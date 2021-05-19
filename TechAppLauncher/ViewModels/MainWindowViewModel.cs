using ReactiveUI;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TechAppLauncher.Services;
using System.Threading;
using System.Linq;
using TechAppLauncher.Models;
using System.IO;
using System.IO.Compression;

namespace TechAppLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ICommand SelectAppCommand { get; }
        public Interaction<AppStoreViewModel, AppViewModel?> ShowAppDialog { get; }
        public ObservableCollection<AppViewModel> Apps { get; } = new();
        public ObservableCollection<AppGalleryViewModel> Galleries { get; } = new();

        public ReactiveCommand<Unit, Unit> LaunchApp { get; }
        public ReactiveCommand<Unit, MainWindowViewModel> CloseWin { get; }

        private CancellationTokenSource? _cancellationTokenSource;

        private RefFileInfo? _refFileInfo;

        private bool _collectionEmpty;
        private bool _isLaunchAble;

        private string _selectedAppId;
        private string _selectedAppUID;
        private string _selectedAppTitle;
        private string _selectedAppVersion;
        private string _selectedAppDescription;
        private string _selectedAppRefFile;

        private IList<RefFileDetail> refFileDetails;


        public bool CollectionEmpty
        {
            get => _collectionEmpty;
            set => this.RaiseAndSetIfChanged(ref _collectionEmpty, value);
        }

        public bool IsLaunchAble
        {
            get => _isLaunchAble;
            set => this.RaiseAndSetIfChanged(ref _isLaunchAble, value);
        }

        public string SelectedAppTitle
        {
            get => _selectedAppTitle;
            set => this.RaiseAndSetIfChanged(ref _selectedAppTitle, value);
        }

        public string SelectedAppUID
        {
            get => _selectedAppUID;
            set => this.RaiseAndSetIfChanged(ref _selectedAppUID, value);
        }

        public string SelectedAppId
        {
            get => _selectedAppId;
            set => this.RaiseAndSetIfChanged(ref _selectedAppId, value);
        }

        public string SelectedAppVersion
        {
            get => _selectedAppVersion;
            set => this.RaiseAndSetIfChanged(ref _selectedAppVersion, value);
        }

        public string SelectedAppDescription
        {
            get => _selectedAppDescription;
            set => this.RaiseAndSetIfChanged(ref _selectedAppDescription, value);
        }

        public string SelectedAppRefFile
        {
            get => _selectedAppRefFile;
            set => this.RaiseAndSetIfChanged(ref _selectedAppRefFile, value);
        }

        public MainWindowViewModel()
        {
            TechAppStoreService techAppStoreService = new TechAppStoreService();
            ShowAppDialog = new Interaction<AppStoreViewModel, AppViewModel?>();


            SelectAppCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var store = new AppStoreViewModel();
                var result = await ShowAppDialog.Handle(store);

                if (refFileDetails == null)
                {
                    refFileDetails = await techAppStoreService.GetAllRefFilesAsync();
                }

                Apps.Clear();

                if (result != null && refFileDetails != null)
                {
                    Apps.Add(result);

                    var refFileDetailSelect = refFileDetails.Where(n => n.AppUID == result.AppId.ToString()).FirstOrDefault();
                    var refFileUrl = refFileDetailSelect != null ? refFileDetailSelect.FileRefUrl : null;

                    if (refFileUrl != null)
                    {
                        _refFileInfo = await techAppStoreService.GetFileAsync(refFileUrl);

                        if (_refFileInfo != null)
                        {
                            refFileUrl = _refFileInfo.FileName;

                            if (_refFileInfo.IsAvailable)
                            {
                                this.IsLaunchAble = true;
                            }
                        }
                    }

                    SelectedAppTitle = result.Title;
                    SelectedAppUID = result.AppUID;
                    SelectedAppId = result.AppId.ToString();
                    SelectedAppVersion = result.AppVersion != null ? result.AppVersion?.ToString() : "";
                    SelectedAppDescription = result.Description;
                    SelectedAppRefFile = refFileUrl;

                    var appGalleries = await techAppStoreService.GetAppDetailGalleries(result.AppUID);

                    if (appGalleries != null)
                    {
                        this.Galleries.Clear();
                        _cancellationTokenSource?.Cancel();
                        _cancellationTokenSource = new CancellationTokenSource();

                        for (int i = 0; i < appGalleries.Count; i++)
                        {
                            var vm = new AppGalleryViewModel(appGalleries[i]);
                            Galleries.Add(vm);
                        }

                        if (!_cancellationTokenSource.IsCancellationRequested)
                        {
                            try
                            {
                                await LoadImage(_cancellationTokenSource.Token);
                            }
                            catch (Exception ex)
                            {
                                string er = ex.Message;
                            }
                        }
                    }
                }
                else
                {
                    this.IsLaunchAble = false;
                }
            });

            CloseWin = ReactiveCommand.Create(() => 
            {
                return this;
            });

            LaunchApp = ReactiveCommand.CreateFromTask(async () =>
            {
                await LaunchApplication();
            });

            this.WhenAnyValue(x => x.Apps.Count)
                .Subscribe(x => CollectionEmpty = x == 0);

        }

        private async Task LoadImage(CancellationToken cancellationToken)
        {
            foreach (var appGallery in Galleries.ToList())
            {
                await appGallery.LoadAppImage();

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        private async Task LaunchApplication()
        {
            this.IsLaunchAble = false;

            string targetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TechAppLauncher");

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }


            //download the file
            string zipFilePath = Path.Combine(targetPath, this._refFileInfo.FileName);
            string workingFolder = zipFilePath.Replace(".zip", "");

            //ensure existing file is remove
            if (Directory.Exists(workingFolder))
            {
                Directory.Delete(workingFolder, true);
            }

            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }

            string workingZipFilePath = Path.Combine(workingFolder, this._refFileInfo.FileName);
            string workingPipFilePath = workingZipFilePath.Replace(".zip", ".pip");

            TechAppStoreService techAppStoreService = new TechAppStoreService();

            if (!File.Exists(workingZipFilePath))
            {
                await techAppStoreService.DownloadFileAsync(this._refFileInfo, workingFolder);
            }

            if (File.Exists(workingPipFilePath))
            {
                //delete pip file
                File.Delete(workingPipFilePath);
            }

            if (File.Exists(workingZipFilePath))
            {
                //extract zip the file
                ZipFile.ExtractToDirectory(workingZipFilePath, workingFolder);

                //delete zip file
                File.Delete(workingZipFilePath);

                if (File.Exists(workingPipFilePath))
                {
                    //rename pip file to zip file
                    //File.Copy(pipFilePath, zipFilePath);

                    //extract pip file
                    ZipFile.ExtractToDirectory(workingPipFilePath, workingFolder);

                    //delete pip file
                    File.Delete(workingPipFilePath);

                    //install plug-in
                    string? ocean2019Home = Environment.GetEnvironmentVariable("Ocean2019Home");
                    string? ocean2019Home_x64 = Environment.GetEnvironmentVariable("OCEAN2019HOME_x64");
                    //string? userProfile = Environment.GetEnvironmentVariable("USERPROFILE");

                    string pluginPackager = Path.Combine(ocean2019Home, "PluginPackager.exe");
                    string petrelFolder = Path.Combine(ocean2019Home_x64, "petrel.exe");
                    string pluginXmlFile = Path.Combine(workingFolder, "Plugin.xml");

                    if (!string.IsNullOrEmpty(ocean2019Home) && !string.IsNullOrEmpty(ocean2019Home_x64))
                    {
                        string processCmd = $"\"{pluginPackager}\" /m \"{pluginXmlFile}\" \"{petrelFolder}\" \"{workingFolder}\"";
                        var result = System.Diagnostics.Process.Start(processCmd);
                        Thread.Sleep(5000);

                        if (result.ExitCode == 0)
                        {
                            //success
                            SelectedAppRefFile = this._refFileInfo.FileName + $" - installed {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
                        }
                        else
                        {

                        }
                    }


                    //remove working folder after all task done
                    //if (Directory.Exists(workingFolder))
                    //{
                    //    Directory.Delete(workingFolder, true);
                    //}
                }
            }
        }
    }
}
