using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Extensions;
using JetReader.Model.View;
using JetReader.Page.Settings.Popups;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Settings {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Reader : ContentPage {
        private MarginEditor _marginEditor;
        private SettingsReaderVm _vm;

        public Reader() {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP) {
                Content.HorizontalOptions = LayoutOptions.Start;
                Content.WidthRequest = 500;
            }

            _marginEditor = new MarginEditor(p => _vm.Margins.Value = p);
            BindingContext = _vm = new SettingsReaderVm();
        }

        private void OpenMarginEditor_OnClicked(object sender, EventArgs e)
        {
            _marginEditor.SetMargins(_vm.Margins.Value);
            _marginEditor.Show();
        }
    }
}