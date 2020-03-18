using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using JetReader.BookLoaders;
using JetReader.Extensions;
using JetReader.Model.Messages;
using JetReader.Service;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Reader {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuickPanel : Rg.Plugins.Popup.Pages.PopupPage
    {
        readonly IMessageBus _messageBus;

        public QuickPanel() {

            _messageBus = IocManager.Container.Resolve<IMessageBus>();

            InitializeComponent();

            BindingContext = new Model.View.QuickPanelVm();

            _messageBus.Subscribe<CloseQuickPanelMessage>(msg => { this.Hide(); });
        }
        
        private void PanelContent_OnChapterChange(object sender, EbookChapter e)
        {
            this.Hide();
        }
    }
}