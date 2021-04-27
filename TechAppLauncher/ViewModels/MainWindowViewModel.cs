using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TechAppLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ICommand SelectAppCommand { get; }
        public Interaction<AppStoreViewModel, AppViewModel?> ShowAppDialog { get; }
        public ObservableCollection<AppViewModel> Apps { get; } = new();

        public ReactiveCommand<Unit, Unit> InstallApp { get; }
        public ReactiveCommand<Unit, MainWindowViewModel> CloseWin { get; }

        private bool _collectionEmpty;


        public bool CollectionEmpty
        {
            get => _collectionEmpty;
            set => this.RaiseAndSetIfChanged(ref _collectionEmpty, value);
        }

        public MainWindowViewModel()
        {
            ShowAppDialog = new Interaction<AppStoreViewModel, AppViewModel?>();

            SelectAppCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var store = new AppStoreViewModel();
                var result = await ShowAppDialog.Handle(store);

                Apps.Clear();

                if (result != null)
                {
                    Apps.Add(result);
                }
            });

            CloseWin = ReactiveCommand.Create(() => 
            {
                return this;
            });

            this.WhenAnyValue(x => x.Apps.Count)
                .Subscribe(x => CollectionEmpty = x == 0);

        }

        private async Task LoadApps()
        {
            var test = "";
            //var apps = (await Album.LoadCachedAsync()).Select(x => new AppViewModel(x));

            //foreach (var album in albums)
            //{
            //    Albums.Add(album);
            //}

            //foreach (var album in Albums.ToList())
            //{
            //    await album.LoadCover();
            //}
        }
    }
}
