using Rg.Plugins.Popup.Pages;

namespace EbookReader.Page.Popups
{
    public partial class LoadingPopup : PopupPage
    {
        public LoadingPopup()
        {
            InitializeComponent();
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}