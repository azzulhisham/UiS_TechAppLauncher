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
using Avalonia;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Timers;
using Avalonia.Controls;
using System.Xml;

namespace TechAppLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ICommand SelectAppCommand { get; }
        public ICommand RemoveAppCommand { get; }

        public Interaction<AppStoreViewModel, AppViewModel?> ShowAppDialog { get; }
        public Interaction<MessageDialogViewModel, MessageDialogViewModel> ShowMsgDialog { get; }
        public Interaction<RemoveAppViewModel, RemoveAppViewModel> ShowRemoveAppDialog { get; }

        public ObservableCollection<AppViewModel> Apps { get; } = new();
        public ObservableCollection<AppGalleryViewModel> Galleries { get; } = new();

        public ObservableCollection<string> ItemsInSystem { get; } = new();


        public ReactiveCommand<Unit, Unit> LaunchApp { get; }
        public ReactiveCommand<Unit, MainWindowViewModel> CloseWin { get; }

        private CancellationTokenSource? _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        private RefFileInfo? _refFileInfo;

        private bool _collectionEmpty;
        private bool _AppInSystemEmpty = true;
        private bool _isLaunchAble;
        private bool _isBusy;

        private string _appTitleBar;

        private string _selectedAppId;
        private string _selectedAppUID;
        private string _selectedAppTitle;
        private string _selectedAppVersion;
        private string _selectedAppDescription;
        private string _selectedAppRefFile;

        private string _installFromFile;
        private string _downloadAppPath;
        private string _selectedItem = "";

        private XmlDocument _xdoc = new XmlDocument();
        private XmlNodeList _xnodes;

        private IList<RefFileDetail> refFileDetails;


        public string SelectedItem
        {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public string AppTitleBar
        {
            get => _appTitleBar;
            set => this.RaiseAndSetIfChanged(ref _appTitleBar, value);
        }

        public bool CollectionEmpty
        {
            get => _collectionEmpty;
            set => this.RaiseAndSetIfChanged(ref _collectionEmpty, value);
        }

        public bool AppInSystemEmpty
        {
            get => _AppInSystemEmpty;
            set => this.RaiseAndSetIfChanged(ref _AppInSystemEmpty, value);
        }

        public bool IsLaunchAble
        {
            get => _isLaunchAble;
            set => this.RaiseAndSetIfChanged(ref _isLaunchAble, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
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

        public string InstallFromFile
        {
            get => _installFromFile;
            set
            {
                this.RaiseAndSetIfChanged(ref _installFromFile, value);
            }
        }

        public string DownloadAppPath
        {
            get => _downloadAppPath;
            set
            {
                this.RaiseAndSetIfChanged(ref _downloadAppPath, value);
            }
        }

        public async Task DoSomething()
        {
            //ShowMsgDialog = new Interaction<MessageDialogViewModel, MessageDialogViewModel>();
            string messageBoxText = _installFromFile;
            var messageBoxDialog = new MessageDialogViewModel(messageBoxText, Enums.MessageBoxStyle.IconStyle.Warning);
            await ShowMsgDialog.Handle(messageBoxDialog);
        }


        public MainWindowViewModel()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version; 
            AppTitleBar = $"Tech App Store - Ver. : {assemblyVersion.Major}.{assemblyVersion.MajorRevision}.{assemblyVersion.Build}.{assemblyVersion.Revision}";

            ITechAppStoreNetworkRequestService techAppStoreService = new TechAppStoreService();
            ShowAppDialog = new Interaction<AppStoreViewModel, AppViewModel?>();
            ShowMsgDialog = new Interaction<MessageDialogViewModel, MessageDialogViewModel>();
            ShowRemoveAppDialog = new Interaction<RemoveAppViewModel, RemoveAppViewModel>();

            SelectAppCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var versionControl = await techAppStoreService.GetLauncherVersion();

                if (versionControl != null)
                {
                    if (assemblyVersion.Major < versionControl.Major || assemblyVersion.MajorRevision < versionControl.MajorRevision ||
                        assemblyVersion.Build < versionControl.Minor || assemblyVersion.Revision < versionControl.MinorRevision)
                    {
                        string messageBoxText = "There is a newer version available.\r\nKindly update your app before start.";
                        var messageBoxDialog = new MessageDialogViewModel(messageBoxText, Enums.MessageBoxStyle.IconStyle.Warning);
                        await ShowMsgDialog.Handle(messageBoxDialog);

                        return;
                    }
                }
                else
                {
                    string messageBoxText = "Your system is not in the correct network.\r\nThis app will not work correctly.";
                    var messageBoxDialog = new MessageDialogViewModel(messageBoxText, Enums.MessageBoxStyle.IconStyle.Warning);
                    await ShowMsgDialog.Handle(messageBoxDialog);
                }

                var store = new AppStoreViewModel();
                var result = await ShowAppDialog.Handle(store);

                if (refFileDetails == null)
                {
                    refFileDetails = await techAppStoreService.GetAllRefFilesAsync();
                }

                bool isLaunchAble = false;
                this.IsLaunchAble = isLaunchAble;
                Apps.Clear();

                if (result != null && refFileDetails != null)
                {
                    Apps.Add(result);

                    var refFileDetailSelect = refFileDetails.Where(n => n.AppUID == result.AppId.ToString()).FirstOrDefault();
                    var refFileUrl = refFileDetailSelect != null ? refFileDetailSelect.FileRefUrl : null;

                    SelectedAppTitle = result.Title;
                    SelectedAppUID = result.AppUID;
                    SelectedAppId = result.AppId.ToString();
                    SelectedAppVersion = result.AppVersion != null ? result.AppVersion?.ToString() : "";
                    SelectedAppDescription = result.Description;
                    SelectedAppRefFile = "";

                    string info = "";
                    if (refFileUrl != null)
                    {
                        _refFileInfo = await techAppStoreService.GetFileAsync(refFileUrl);

                        if (_refFileInfo != null)
                        {
                            refFileUrl = _refFileInfo.FileName;
                            SelectedAppRefFile = refFileUrl;

                            if (!string.IsNullOrEmpty(refFileUrl) && _refFileInfo.IsAvailable)
                            {
                                var userName = Environment.UserName;
                                var userDownloadSessions = await techAppStoreService.GetUserDownloadSessionByUser(userName.Replace(".", "_"));

                                if (userDownloadSessions != null && userDownloadSessions.Count > 0)
                                {
                                    if (userDownloadSessions.Any(n => n.AppUID.ToLower().Trim() == result.AppUID.ToLower().Trim()))
                                    {
                                        var chkUserDownloadSessions = userDownloadSessions.Where(n => n.AppUID.ToLower().Trim() == result.AppUID.ToLower().Trim() && n.Status.Trim().ToLower() == "completed").ToList();

                                        if (chkUserDownloadSessions != null && chkUserDownloadSessions.Count > 0)
                                        {
                                            info = $"The Plug-in has been installed on {userDownloadSessions[0].InstallTimeStamp.ToString("yyyy-MM-dd HH:mm:ss")}";
                                        }
                                    }
                                }

                                isLaunchAble = true;
                            }
                        }
                    }

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

                    if (!string.IsNullOrEmpty(info))
                    {
                        var messageBoxDialog = new MessageDialogViewModel(info, Enums.MessageBoxStyle.IconStyle.Info);
                        await ShowMsgDialog.Handle(messageBoxDialog);
                    }

                    this.IsLaunchAble = isLaunchAble;
                }

                LoadXmlContent();
            });

            CloseWin = ReactiveCommand.Create(() => 
            {
                return this;
            });

            LaunchApp = ReactiveCommand.CreateFromTask(async () =>
            {
                await LaunchApplication();
                LoadXmlContent();
            });

            RemoveAppCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var removeApp = new RemoveAppViewModel(SelectedItem);
                var result = await ShowRemoveAppDialog.Handle(removeApp);
                LoadXmlContent();
            });

            this.WhenAnyValue(x => x.Apps.Count)
                .Subscribe(x => CollectionEmpty = x == 0);

            LoadXmlContent();
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

        public async Task RaiseMessage(string msg)
        {
            var messageBoxDialog = new MessageDialogViewModel(msg, Enums.MessageBoxStyle.IconStyle.Warning);
            await ShowMsgDialog.Handle(messageBoxDialog);
        }

        public async Task LaunchApplication(string filePath = "")
        {
            this.IsLaunchAble = false;
            this.IsBusy = true;

            string messageBoxText = "";
            string targetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TechAppLauncher");

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }


            //file attached option
            if (!string.IsNullOrEmpty(filePath))
            {
                this._refFileInfo = new RefFileInfo()
                {
                    FileName = Path.GetFileName(filePath)
                };
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

            ITechAppStoreNetworkRequestService techAppStoreService = new TechAppStoreService();

            if (!File.Exists(workingZipFilePath))
            {
                //Do not download from sever whenever attachment is provided
                if (string.IsNullOrEmpty(filePath))
                {
                    await techAppStoreService.DownloadFileAsync(this._refFileInfo, workingFolder);
                }
                else
                {
                    File.Copy(filePath, workingZipFilePath, true);

                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        string er = ex.Message;
                    }
                }
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

                    if (!string.IsNullOrEmpty(ocean2019Home) && !string.IsNullOrEmpty(ocean2019Home_x64))
                    {
                        try
                        {
                            string pluginPackager = Path.Combine(ocean2019Home, "PluginPackager.exe");
                            string petrelFolder = Path.Combine(ocean2019Home_x64, "petrel.exe");
                            string pluginXmlFile = Path.Combine(workingFolder, "Plugin.xml");

                            string processCmd = $"\"{pluginPackager}\" /m \"{pluginXmlFile}\" \"{petrelFolder}\" \"{workingFolder}\"";

                            _cancellationToken = new CancellationToken();

                            var process = System.Diagnostics.Process.Start(processCmd);
                            //process.StartInfo.CreateNoWindow = true;

                            await process.WaitForExitAsync(_cancellationToken);

                            await Task.Run(() => Sleep(5));

                            if (process.ExitCode == 0)
                            {
                                //success
                                SelectedAppRefFile = this._refFileInfo.FileName + $" - installed {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
                                messageBoxText = "Success! \r\nThe Plug-in has been installed";

                                var rec = techAppStoreService.AddUserDownloadSession(new UserDownloadSession()
                                {
                                    AppId = Convert.ToInt64(SelectedAppId),
                                    AppUID = SelectedAppUID,
                                    Title = SelectedAppTitle,
                                    UserName = Environment.UserName,
                                    Status = "completed",
                                    Remark = "",
                                    InstallTimeStamp = DateTime.Now
                                });
                            }
                            else
                            {
                                if (Directory.Exists(workingFolder))
                                {
                                    Directory.Delete(workingFolder, true);
                                }

                                messageBoxText = "Fail! \r\n" + $"ExitCode = {process.ExitCode}";
                            }

                        }
                        catch (Exception ex)
                        {
                            if (Directory.Exists(workingFolder))
                            {
                                Directory.Delete(workingFolder, true);
                            }

                            messageBoxText = "Fail! \r\n" + ex.Message;
                        }
                    }
                    else
                    {
                        if (Directory.Exists(workingFolder))
                        {
                            Directory.Delete(workingFolder, true);
                        }

                        messageBoxText = "Ops! Petrel is not available on your system.";
                        await Task.Run(() => Sleep(3));
                    }

                    var messageBoxDialog = new MessageDialogViewModel(messageBoxText, (messageBoxText.ToLower().StartsWith("success") ? Enums.MessageBoxStyle.IconStyle.Success : Enums.MessageBoxStyle.IconStyle.Error));
                    await ShowMsgDialog.Handle(messageBoxDialog);
                }
                else
                {
                    if (Directory.Exists(workingFolder))
                    {
                        Directory.Delete(workingFolder, true);
                    }

                    var messageBoxDialog = new MessageDialogViewModel("Ops! Looks like this installation is currently not supported.\r\nKindly refer to the Tech. App. Developer.", Enums.MessageBoxStyle.IconStyle.Error);
                    await ShowMsgDialog.Handle(messageBoxDialog);
                }
            }
            else
            {
                if (Directory.Exists(workingFolder))
                {
                    Directory.Delete(workingFolder, true);
                }

                var messageBoxDialog = new MessageDialogViewModel("Ops! Looks like there is some techincal issues exists on your system.\r\nKindly refer to the Tech. App. Developer.", Enums.MessageBoxStyle.IconStyle.Error);
                await ShowMsgDialog.Handle(messageBoxDialog);
            }

            Apps.Clear();
            this.IsBusy = false;
        }

        public async Task DownloadApplication()
        {
            this.IsLaunchAble = false;
            this.IsBusy = true;

            string messageBoxText = "";
            string targetPath = DownloadAppPath;

            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            string workingZipFilePath = Path.Combine(targetPath, this._refFileInfo.FileName);

            ITechAppStoreNetworkRequestService techAppStoreService = new TechAppStoreService();

            if (!File.Exists(workingZipFilePath))
            {
                await techAppStoreService.DownloadFileAsync(this._refFileInfo, targetPath);
            }

            if (File.Exists(workingZipFilePath))
            {
                //success
                SelectedAppRefFile = this._refFileInfo.FileName + $" - downloaded {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
                messageBoxText = "Success! \r\nThe Plug-in has been downloaded";

                var rec = techAppStoreService.AddUserDownloadSession(new UserDownloadSession()
                {
                    AppId = Convert.ToInt64(SelectedAppId),
                    AppUID = SelectedAppUID,
                    Title = SelectedAppTitle,
                    UserName = Environment.UserName,
                    Status = "downloaded",
                    Remark = "",
                    InstallTimeStamp = DateTime.Now
                });

            
                var messageBoxDialog = new MessageDialogViewModel(messageBoxText, (messageBoxText.ToLower().StartsWith("success") ? Enums.MessageBoxStyle.IconStyle.Success : Enums.MessageBoxStyle.IconStyle.Error));
                await ShowMsgDialog.Handle(messageBoxDialog);
            }
            else
            {
                var messageBoxDialog = new MessageDialogViewModel("Ops! Looks like there is some techincal issues exists on your system.\r\nKindly refer to the Tech. App. Developer.", Enums.MessageBoxStyle.IconStyle.Error);
                await ShowMsgDialog.Handle(messageBoxDialog);
            }

            Apps.Clear();
            this.IsBusy = false;
        }

        private async Task Sleep(int seconds)
        {
            DateTime now = DateTime.Now;

            while (DateTime.Now < now.AddSeconds(seconds))
            {
                Thread.Sleep(1);
            }
        }

        public void LoadXmlContent()
        {
            IXmlDocService xmlDocService = new XmlDocService();
            var result = xmlDocService.XmlLoad(ItemsInSystem);

            AppInSystemEmpty = result > 0 ? false : true;
        }
    }
}
