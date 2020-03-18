using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.Books;
using JetReader.DependencyService;
using JetReader.Model.Format;
using JetReader.Model.Messages;
using JetReader.Service;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Home {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BookCard : StackLayout {

        private readonly BookInfo _book;

        public event Action<BookInfo> OnOpenBook;
        public event Action<BookInfo> OnDeleteBook;

        public BookCard(BookInfo book) {

            _book = book;

            StyleId = book.Id;

            BindingContext = new {
                book.Title,
                Width = Card.CardWidth,
                Height = Card.CardHeight,
                PlaceholderWidth = Card.CardWidth * .75,
                IsFinished = book.FinishedReading.HasValue
            };

            InitializeComponent();

            LoadImage();

            DeleteIcon.GestureRecognizers.Add(
                new TapGestureRecognizer {
                    Command = new Command(Delete)
                }
            );

            GestureRecognizers.Add(
                new TapGestureRecognizer {
                    Command = new Command(Open),
                });
        }

        private void LoadImage() {
            if (string.IsNullOrEmpty(_book.CoverFilename)) return;

            Cover.Source = ImageSource.FromFile(_book.GetTempPath(_book.CoverFilename));
            Cover.Aspect = Aspect.Fill;
            Cover.WidthRequest = Card.CardWidth;
            Cover.HeightRequest = Card.CardHeight;
        }

        private void Open() {
            OnOpenBook?.Invoke(_book);
        }

        private void Delete()
        {
            OnDeleteBook?.Invoke(_book);
        }

    }
}