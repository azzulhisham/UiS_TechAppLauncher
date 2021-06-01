using ReactiveUI;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static TechAppLauncher.Enums.MessageBoxIconStyle;

namespace TechAppLauncher.ViewModels
{
    public class MessageDialogViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, MessageDialogViewModel> CloseWin { get; }

        private string _messageText;
        private string _windowColor = "WhiteSmoke";
        private Bitmap? _icoImg;
        
        public MessageDialogViewModel()
        {
            CloseWin = ReactiveCommand.Create(() =>
            {
                return this;
            });
        }

        public MessageDialogViewModel(string messageText, IconStyle iconStyle)
        {
            MessageText = messageText;
            WindowColor = "WhiteSmoke";

            CloseWin = ReactiveCommand.Create(() =>
            {
                return this;
            });

            LoadAppImage(iconStyle);
        }

        public string MessageText
        {
            get => _messageText;
            set => this.RaiseAndSetIfChanged(ref _messageText, value);
        }

        public string WindowColor
        {
            get => _windowColor;
            set => this.RaiseAndSetIfChanged(ref _windowColor, value);
        }


        public Bitmap? IcoImg
        {
            get => _icoImg;
            private set => this.RaiseAndSetIfChanged(ref _icoImg, value);
        }

        public async Task LoadAppImage(IconStyle iconStyle)
        {
            string iconPath = "";

            if (iconStyle == IconStyle.Success)
            {
                iconPath = "success.png";
            }

            if (iconStyle == IconStyle.Error)
            {
                iconPath = "error.png";
            }

            if (iconStyle == IconStyle.Info)
            {
                iconPath = "info.png";
            }

            if (iconStyle == IconStyle.Warning)
            {
                iconPath = "warning.png";
            }

            try
            {
                await using (var imageStream = File.OpenRead(iconPath))
                {
                    if (imageStream != null)
                    {
                        IcoImg = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
                    }
                }
            }
            catch (Exception dx)
            {

            }
        }

    }
}
