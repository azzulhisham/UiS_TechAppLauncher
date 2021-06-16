using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TechAppLauncher.ViewModels;

namespace TechAppLauncher.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated(d => d(ViewModel.CloseWin.Subscribe(Close)));
            this.WhenActivated(d => d(ViewModel.ShowAppDialog.RegisterHandler(DoShowAppDialogAsync)));
            this.WhenActivated(d => d(ViewModel.ShowMsgDialog.RegisterHandler(DoShowMsgDialogAsync)));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void OnDownloadAppClicked(object sender, RoutedEventArgs args)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            string result = await openFolderDialog.ShowAsync(this);

            if (string.IsNullOrEmpty(result))
            {
                return;
            }

            var context = this.DataContext as MainWindowViewModel;
            context.DownloadAppPath = result;
            await context.DownloadApplication();
        }

        public async void OnInstallAppFromFileClicked(object sender, RoutedEventArgs args)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            string[] result = await openFileDialog.ShowAsync(this);

            if (result.Length == 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(result[0]))
            {
                return;
            }

            var context = this.DataContext as MainWindowViewModel;
            context.InstallFromFile = result[0];
            await context.LaunchApplication(result[0]);
        }

        private async Task DoShowAppDialogAsync(InteractionContext<AppStoreViewModel, AppViewModel?> interaction)
        {
            var appStoreDialog = new AppStoreWindow();
            appStoreDialog.DataContext = interaction.Input;

            var result = await appStoreDialog.ShowDialog<AppViewModel?>(this);
            interaction.SetOutput(result);
        }

        private async Task DoShowMsgDialogAsync(InteractionContext<MessageDialogViewModel, MessageDialogViewModel> interaction)
        {
            var msgDialog = new MessageDialogView();
            msgDialog.DataContext = interaction.Input;

            var result = await msgDialog.ShowDialog<MessageDialogViewModel>(this);
            interaction.SetOutput(result);
        }
    }
}
