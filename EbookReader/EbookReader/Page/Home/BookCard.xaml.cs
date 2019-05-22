﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EbookReader.DependencyService;
using EbookReader.Model.Messages;
using EbookReader.Service;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page.Home {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BookCard : StackLayout {

        private readonly Model.Bookshelf.Book _book;

        public BookCard(Model.Bookshelf.Book book) {

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

            var fileService = IocManager.Container.Resolve<FileService>();
            Cover.Source = ImageSource.FromFile(_book.GetTempPath(_book.CoverFilename));
            Cover.Aspect = Aspect.Fill;
            Cover.WidthRequest = Card.CardWidth;
            Cover.HeightRequest = Card.CardHeight;
        }

        private void Open() {
            var messageBus = IocManager.Container.Resolve<IMessageBus>();
            messageBus.Send(new OpenBookMessage { Book = _book });
        }

        private void Delete() {
            var messageBus = IocManager.Container.Resolve<IMessageBus>();
            messageBus.Send(new DeleteBookMessage { Book = _book });
        }

    }
}