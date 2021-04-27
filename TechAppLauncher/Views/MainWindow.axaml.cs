using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
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
    }
}
