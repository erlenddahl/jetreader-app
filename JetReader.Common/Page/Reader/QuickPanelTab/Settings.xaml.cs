using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.Config.CommandGrid;
using JetReader.DependencyService;
using JetReader.Service;
using JetReader.Model.Messages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using JetReader.Model.View;

namespace JetReader.Page.Reader.QuickPanelTab {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : StackLayout {

        private readonly IMessageBus _messageBus;

        public Settings() {

            // IOC
            _messageBus = IocManager.Container.Resolve<IMessageBus>();

            InitializeComponent();

            if (Device.RuntimePlatform == Device.Android) {
                FontPicker.WidthRequest = 75;
                FontPicker.Title = "Font size";

                MarginPicker.WidthRequest = 75;
                MarginPicker.Title = "Margin";

                var brightnessProvider = IocManager.Container.Resolve<IBrightnessProvider>();
                Brightness.Value = brightnessProvider.Brightness * 100;
            }

            if (Device.RuntimePlatform == Device.UWP) {
                BrightnessWrapper.IsVisible = false;
            }

        }

        private void Brightness_ValueChanged(object sender, ValueChangedEventArgs e) {
            if (e.OldValue != e.NewValue) {
                _messageBus.Send(new ChangeBrightnessMessage(e.NewValue / 100d));
            }
        }

        private void Switch_OnToggled(object sender, ToggledEventArgs e)
        {
            _messageBus.Send(new ChangeThemeMessage(Theme.DefaultThemes[e.Value ? 0 : 1])); //TODO: Fix this
        }
    }
}