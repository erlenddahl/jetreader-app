using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using EbookReader.Model.Messages;
using EbookReader.Service;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page.Reader {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuickPanel : StackLayout {
        readonly IMessageBus _messageBus;

        public QuickPanel() {

            _messageBus = IocManager.Container.Resolve<IMessageBus>();

            InitializeComponent();

            BindingContext = new Model.View.QuickPanelVm();

            _messageBus.Subscribe<CloseQuickPanelMessage>((msg) => Hide());
        }
        
        private void PanelContent_OnChapterChange(object sender, Model.Navigation.Item e) {
            Hide();
        }

        public void Show() {
            Device.BeginInvokeOnMainThread(() => {
                IsVisible = true;
            });
        }

        public void Hide() {
            Device.BeginInvokeOnMainThread(() => {
                IsVisible = false;
            });
        }
    }
}