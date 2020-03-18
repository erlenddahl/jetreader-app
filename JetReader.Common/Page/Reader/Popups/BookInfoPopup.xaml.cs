using System.Collections.Generic;
using System.Threading.Tasks;
using JetReader.Books;
using JetReader.Model.Format;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Reader.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BookInfoPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public BookInfoPopup(Ebook book)
        {
            BindingContext = this;

            InitializeComponent();

            LoadImage(book);
            
            foreach (var item in book.GetInfo().Result)
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
    }
}