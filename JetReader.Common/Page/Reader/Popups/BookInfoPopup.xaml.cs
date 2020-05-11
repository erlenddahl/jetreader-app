using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions.TimeSpanExtensions;
using JetReader.Books;
using JetReader.Extensions;
using JetReader.Model.Format;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Reader.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BookInfoPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public Ebook Book { get; }

        public BookInfoPopup(Ebook book)
        {
            Book = book;
            BindingContext = this;

            InitializeComponent();

            DisplayInfo();
        }

        private async void DisplayInfo()
        {
            LoadImage(Book);

            await Book.Info.WaitForProcessingToFinish();

            foreach (var item in Book.GetInfo().Result)
            {
                AddInfoControl(container, item);
            }

            AddEmpty(container);
            foreach (var item in Book.Info.ReadStats.Dates.Select(p => (p.Date.ToShortDateString(), $"{TimeSpan.FromSeconds(p.Seconds).ToShortPrettyFormat()}, {p.Words:n0} words, {(p.Words / (p.Seconds / 60)):n0} words/min.")))
            {
                AddInfoControl(container, item);
            }
        }

        private void AddInfoControl(StackLayout parent, (string title, string value) item, double marginLeft = 0)
        {
            if (string.IsNullOrWhiteSpace(item.title))
            {
                AddEmpty(parent);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(item.value)) return;
                var l = new Label()
                {
                    FormattedText = new FormattedString()
                    {
                        Spans =
                        {
                            new Span() {Text = item.title + ": ", FontAttributes = FontAttributes.Bold, FontSize = 16},
                            new Span() {Text = item.value, FontSize = 16}
                        }
                    }
                };
                l.Margin = new Thickness(marginLeft, l.Margin.Top, l.Margin.Right, l.Margin.Bottom);
                parent.Children.Add(l);
            }
        }

        private void AddEmpty(StackLayout parent)
        {
            parent.Children.Add(new Label());
        }

        private void LoadImage(Ebook book)
        {
            if (string.IsNullOrEmpty(book.CoverFilename)) return;

            Cover.Source = ImageSource.FromFile(book.Info.GetTempPath(book.CoverFilename));
            Cover.Aspect = Aspect.AspectFit;
        }

        private void CloseButton_OnClicked(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void ImageTapped(object sender, EventArgs e)
        {
            ImagePreviewPopup.Enlarge(Cover);
        }
    }
}