using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Model.View;
using JetReader.Page.Settings.Popups;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Settings {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Control : ContentPage {

        private CommandGridConfigPopup _commandGridPopup;

        public Control() {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP) {
                Content.HorizontalOptions = LayoutOptions.Start;
                Content.WidthRequest = 500;
            }

            BindingContext = new SettingsControlVm();

            _commandGridPopup = new CommandGridConfigPopup();
        }

        private void EditCommandGrid_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PushAsync(_commandGridPopup);
        }
    }
}