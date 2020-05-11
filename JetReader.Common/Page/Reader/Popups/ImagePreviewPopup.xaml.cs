using System;
using System.Threading.Tasks;
using JetReader.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Reader.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePreviewPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public ImagePreviewPopup()
        {
            InitializeComponent();
        }

        private void CancelButton_OnClicked(object sender, EventArgs e)
        {
            this.Hide();
        }

        public static Task Enlarge(Image cover)
        {
            return FromSource(cover.Source);
        }

        private static Task FromSource(ImageSource coverSource)
        {
            var popup = new ImagePreviewPopup();
            popup.LargeImage.Source = coverSource;
            return popup.Show();
        }
    }
}