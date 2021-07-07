using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TechAppLauncher.Services;

namespace TechAppLauncher.ViewModels
{
    public class RemoveAppViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> RemoveApp { get; }
        public ReactiveCommand<Unit, RemoveAppViewModel> CloseWin { get; }
        public Interaction<MessageDialogViewModel, MessageDialogViewModel> ShowMsgDialog { get; }

        public ObservableCollection<string> ItemsInSystem { get; } = new();

        IXmlDocService xmlDocService;
        string selectedItem = "";

        public string SelectedItem
        {
            get => selectedItem;
            set => this.RaiseAndSetIfChanged(ref selectedItem, value);
        }

        public RemoveAppViewModel()
        {
            xmlDocService = new XmlDocService();
            ShowMsgDialog = new Interaction<MessageDialogViewModel, MessageDialogViewModel>();

            CloseWin = ReactiveCommand.Create(() =>
            {
                return this;
            });

            RemoveApp = ReactiveCommand.CreateFromTask(async () =>
            {
                var removeIndex = ItemsInSystem.IndexOf(SelectedItem);

                if (removeIndex < 0)
                {
                    string messageBoxText = "Please select an app in order to start uninstalling.";
                    var messageBoxDialog = new MessageDialogViewModel(messageBoxText, Enums.MessageBoxStyle.IconStyle.Info);
                    var result = await ShowMsgDialog.Handle(messageBoxDialog);
                }
                else
                {
                    string messageBoxText = "Are you sure you want to uninstall this app?";
                    var messageBoxDialog = new MessageDialogViewModel(messageBoxText, Enums.MessageBoxStyle.IconStyle.Warning, Enums.MessageBoxStyle.ButtonStyle.YesNo, Enums.MessageBoxStyle.DefaultButton.Button2);
                    var result = await ShowMsgDialog.Handle(messageBoxDialog);

                    if (result != null && result.ButtonResult == Enums.MessageBoxStyle.ButtonResult.Yes)
                    {
                        xmlDocService.XmlRemoveItem(removeIndex);
                        ItemsInSystem.Remove(SelectedItem);

                        LoadXmlContent();
                    }
                }
            });

            LoadXmlContent();
        }

        private void LoadXmlContent() 
        {         
            var result = xmlDocService.XmlLoad(ItemsInSystem);
        }
    }
}
