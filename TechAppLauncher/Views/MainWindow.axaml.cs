using Avalonia;
using Avalonia.Controls;
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
