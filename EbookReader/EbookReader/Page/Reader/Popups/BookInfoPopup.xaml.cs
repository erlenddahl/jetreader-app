using System.Collections.Generic;
using System.Threading.Tasks;
using EbookReader.Books;
using EbookReader.Model.Format;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page.Reader.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BookInfoPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public BookInfoPopup(Ebook book)
        {
            BindingContext = this;

            InitializeComponent();

            LoadImage(book);
            
            foreach (var item in book.GetInfo())
            {
                if (string.IsNullOrWhiteSpace(item.title))
                {
                    container.Children.Add(new Label());
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(item.value)) continue;
                    var box = new StackLayout() { Orientation = StackOrientation.Horizontal };
                    box.Children.Add(new Label()
                    {
                        FormattedText = new FormattedString()
                        {
                            Spans =
                            {
                                new Span(){ Text = item.title + ": ", FontAttributes = FontAttributes.Bold, FontSize = 16},
                                new Span(){ Text = item.value, FontSize = 16 }
                            }
                        }
                    });
                    container.Children.Add(box);
                }
            }
        }

        private void LoadImage(Ebook book)
        {
            if (string.IsNullOrEmpty(book.CoverFilename)) return;

            Cover.Source = ImageSource.FromFile(book.Info.GetTempPath(book.CoverFilename));
            Cover.Aspect = Aspect.AspectFit;
        }

        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.RemovePageAsync(this, false);
        }
    }
}