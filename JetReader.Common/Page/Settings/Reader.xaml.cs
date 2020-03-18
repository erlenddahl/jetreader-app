using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Model.View;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Settings {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Reader : ContentPage {
        public Reader() {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP) {
                Content.HorizontalOptions = LayoutOptions.Start;
                Content.WidthRequest = 500;
            }

            BindingContext = new SettingsReaderVm();
        }
    }
}