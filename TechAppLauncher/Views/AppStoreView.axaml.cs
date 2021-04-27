using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TechAppLauncher.Views
{
    public class AppStoreView : UserControl
    {
        public AppStoreView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
