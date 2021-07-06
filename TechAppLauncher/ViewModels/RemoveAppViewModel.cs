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

namespace TechAppLauncher.ViewModels
{
    public class RemoveAppViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> RemoveApp { get; }
        public ReactiveCommand<Unit, RemoveAppViewModel> CloseWin { get; }
        public Interaction<MessageDialogViewModel, MessageDialogViewModel> ShowMsgDialog { get; }

        public ObservableCollection<string> ItemsInSystem { get; } = new();

        XmlDocument xdoc = new XmlDocument();
        XmlNodeList xnodes;
        string selectedItem = "";

        public string SelectedItem
        {
            get => selectedItem;
            set => this.RaiseAndSetIfChanged(ref selectedItem, value);
        }

        public RemoveAppViewModel()
        {
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
                        XmlNode item = xnodes[removeIndex];
                        item.ParentNode.RemoveChild(item);

                        ItemsInSystem.Remove(SelectedItem);

                        xdoc.Save(@"C:\Users\zulhisham\Downloads\PluginManagerSettings1.xml");
                        LoadXmlContent();
                    }
                }
            });

            LoadXmlContent();
        }

        private void LoadXmlContent() 
        {
            xdoc.Load(@"C:\Users\zulhisham\Downloads\PluginManagerSettings.xml");
            xnodes = xdoc.GetElementsByTagName("Plugin");

            ItemsInSystem.Clear();

            foreach (XmlNode item in xnodes)
            {
                var name = item.SelectSingleNode("Name") == null ? " - " : item.SelectSingleNode("Name").InnerText;
                var typeNames = item.SelectSingleNode("PluginTypeName") == null ? " - " : item.SelectSingleNode("PluginTypeName").InnerText;

                string[] typeName = typeNames.Split(new char[] { ',' });

                ItemsInSystem.Add($"{name}       :{typeName[1].Trim()}");
            }
        }
    }
}
