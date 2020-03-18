using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.Model;
using JetReader.Model.Messages;
using JetReader.Model.View;
using JetReader.Service;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Settings {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Synchronization : ContentPage {
        readonly SettingsSynchronizationVm _vm;

        public Synchronization() {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP) {
                Content.HorizontalOptions = LayoutOptions.Start;
                Content.WidthRequest = 500;
            }

            _vm = new SettingsSynchronizationVm();

            BindingContext = _vm;

            IocManager.Container.Resolve<IMessageBus>().Subscribe<OpenDropboxLoginMessage>(OpenDropboxLogin);

            IocManager.Container.Resolve<IMessageBus>().Subscribe(async (OAuth2LoginPageClosed msg) => {
                if (msg.Provider == "Dropbox") {
                    await Navigation.PopModalAsync();
                }
            });
        }

        private async void OpenDropboxLogin(OpenDropboxLoginMessage msg) {

            var oAuth2Data = new OAuth2RequestData {
                Provider = "Dropbox",
                ClientId = AppSettings.Synchronization.Dropbox.ClientId,
                AuthorizeUrl = "https://www.dropbox.com/oauth2/authorize",
                RedirectUrl = AppSettings.Synchronization.Dropbox.RedirectUrl,
            };

            Xamarin.Forms.Application.Current.Properties["OAuth2Data"] = oAuth2Data;

            await Navigation.PushModalAsync(new OAuth2LoginPage());
        }

        private void Email_Completed(object sender, EventArgs e) {
            FirebasePassword.Focus();
        }

        private void Password_Completed(object sender, EventArgs e) {
            if (_vm != null && _vm.Firebase != null && _vm.Firebase.ConnectCommand != null && !string.IsNullOrEmpty(_vm.Firebase.Email) && !string.IsNullOrEmpty(_vm.Firebase.Password)) {
                _vm.Firebase.ConnectCommand.Execute(null);
            }
        }
    }
}