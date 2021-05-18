using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TechAppLauncher.Services;

namespace TechAppLauncher.ViewModels
{
    public class AppStoreViewModel: ViewModelBase
    {
        private AppViewModel? _selectedApp;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isLaunchAble;

        public ObservableCollection<AppViewModel> SelectedResults { get; } = new();
        public ReactiveCommand<Unit, AppViewModel?> GetAppSelectCommand { get; }
        public ReactiveCommand<Unit, AppViewModel?> GetAppSelectCommandClose { get; }


        public AppViewModel? SelectedApp
        {
            get => _selectedApp;
            set 
            {
                this.IsLaunchAble = true;
                this.RaiseAndSetIfChanged(ref _selectedApp, value);
            }
        }

        public bool IsLaunchAble
        {
            get => _isLaunchAble;
            set => this.RaiseAndSetIfChanged(ref _isLaunchAble, value);
        }

        public AppStoreViewModel()
        {
            GetAppSelectCommand = ReactiveCommand.Create(() =>
            {
                return SelectedApp;
            });

            GetAppSelectCommandClose = ReactiveCommand.Create(() =>
            {
                SelectedApp = null;
                return SelectedApp;
            });

            ListAllApp();
        }

        private async void ListAllApp()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                //Call API
                TechAppStoreService techAppStoreService = new TechAppStoreService();
                var apps = await techAppStoreService.GetAllAsync();

                foreach (var app in apps)
                {
                    var vm = new AppViewModel(app);

                    SelectedResults.Add(vm);
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
            catch (Exception ex)
            {

            }
        }

        private async Task LoadImage(CancellationToken cancellationToken)
        {
            foreach (var app in SelectedResults.ToList())
            {
                await app.LoadAppImage();

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}
