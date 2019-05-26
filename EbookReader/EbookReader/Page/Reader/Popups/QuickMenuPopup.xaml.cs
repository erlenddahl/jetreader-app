using System;
using System.Collections.Generic;
using Autofac;
using EbookReader.Config.CommandGrid;
using EbookReader.Model.Messages;
using EbookReader.Service;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace EbookReader.Page.Reader.Popups
{
    public partial class QuickMenuPopup : PopupPage
    {
        private IMessageBus _messageBus;
        ThemePickerPopup _themePicker = new ThemePickerPopup();

        public Theme SelectedTheme
        {
            get => UserSettings.Reader.Theme;
            set
            {
                UserSettings.Reader.Theme = value;
                _messageBus.Send(new ChangeThemeMessage(value));
                OnPropertyChanged();
            }
        }

        public double Brightness
        {
            get => UserSettings.Reader.Brightness * 100d;
            set
            {
                UserSettings.Reader.Brightness = value / 100d;
                _messageBus.Send(new ChangesBrightnessMessage(value / 100d));
                OnPropertyChanged();
            }
        }

        public double FontSize
        {
            get => UserSettings.Reader.FontSize;
            set
            {
                UserSettings.Reader.FontSize = value;
                _messageBus.Send(new ChangeFontSizeMessage(value));
                OnPropertyChanged();
            }
        }

        public QuickMenuPopup()
        {
            _messageBus = IocManager.Container.Resolve<IMessageBus>();

            InitializeComponent();

            var tap = new TapGestureRecognizer();
            tap.Tapped += async (sender, e) => {
                await Navigation.PushPopupAsync(_themePicker);
            };
            themeRow.GestureRecognizers.Add(tap);

            _themePicker.ThemeSelected += theme => { SelectedTheme = theme; };

            BindingContext = this;
        }

        private void BrightnessDownClicked(object sender, EventArgs e)
        {
            Brightness -= 5;
        }

        private void BrightnessUpClicked(object sender, EventArgs e)
        {
            Brightness += 5;
        }

        private void FontSizeDownClicked(object sender, EventArgs e)
        {
            FontSize -= 1;
        }

        private void FontSizeUpClicked(object sender, EventArgs e)
        {
            FontSize += 1;
        }
    }
}