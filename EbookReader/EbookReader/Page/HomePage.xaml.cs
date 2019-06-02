using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EbookReader.Books;
using EbookReader.Helpers;
using EbookReader.Model.Messages;
using EbookReader.Page.Home;
using EbookReader.Page.Popups;
using EbookReader.Service;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Plugin.FilePicker;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : ContentPage {
        readonly IBookshelfService _bookshelfService;
        readonly IMessageBus _messageBus;

        LoadingPopup _loadingPopup = new LoadingPopup();

        public HomePage() {
            InitializeComponent();

            // ioc
            _bookshelfService = IocManager.Container.Resolve<IBookshelfService>();
            _messageBus = IocManager.Container.Resolve<IMessageBus>();

            if (!App.HasMasterDetailPage) {

                var settingsItem = new ToolbarItem {
                    Text = "Settings",
                    Icon = "settings.png"
                };
                settingsItem.Clicked += SettingsItem_Clicked;
                ToolbarItems.Add(settingsItem);

                var aboutItem = new ToolbarItem {
                    Text = "About",
                    Icon = "info.png",
                };
                aboutItem.Clicked += AboutItem_Clicked;

                ToolbarItems.Add(aboutItem);

            }
        }

        protected override void OnAppearing() {
            base.OnAppearing();

            // because of floating action button on android
            Xamarin.Forms.Device.StartTimer(new TimeSpan(0, 0, 0, 0, 200), () => {
                _messageBus.Send(new FullscreenRequestMessage(false, false));
                return false;
            });

            UserSettings.FirstRun = false;

            _messageBus.Subscribe<AddBookClickedMessage>(AddBook, nameof(HomePage));

            LoadBookshelf();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            PopupNavigation.Instance.RemovePageAsync(_loadingPopup, false);
            _messageBus.UnSubscribe(nameof(HomePage));
        }

        private async void AboutItem_Clicked(object sender, EventArgs e) {
            await Navigation.PushAsync(new AboutPage());
        }

        private async void SettingsItem_Clicked(object sender, EventArgs e) {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void LoadBookshelf() {

            Bookshelf.Children.Clear();

            var books = await _bookshelfService.LoadBooks();

            foreach (var book in books) {
                var bc = new BookCard(book);
                bc.OnOpenBook += OpenBook;
                bc.OnDeleteBook += DeleteBook;
                Bookshelf.Children.Add(bc);
            }
        }

        private async void AddBook(AddBookClickedMessage msg)
        {

            var permissionStatus = await PermissionHelper.CheckAndRequestPermission(Plugin.Permissions.Abstractions.Permission.Storage);

            if (permissionStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                await DisplayAlert("Permission not granted", "Cannot open book without storage permissions.", "OK");
                return;
            }

            var pickedFile = await CrossFilePicker.Current.PickFile();
            if (pickedFile == null) return;

            await Navigation.PushPopupAsync(_loadingPopup);

            try
            {
                var (book, isNew) = await _bookshelfService.AddBook(pickedFile);
                if (isNew)
                {
                    Bookshelf.Children.Add(new BookCard(book));
                }

                await Navigation.RemovePopupPageAsync(_loadingPopup);
                SendBookToReader(book);
            }
            catch (Exception e)
            {
                var ext = string.Empty;
                if (!string.IsNullOrEmpty(pickedFile.FileName))
                    ext = System.IO.Path.GetExtension(pickedFile.FileName);

                Analytics.TrackEvent("Failed to open book", new Dictionary<string, string>
                {
                    {"Extension", ext}
                });
                Crashes.TrackError(e, new Dictionary<string, string>
                {
                    {"Filename", pickedFile.FileName}
                });
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                await DisplayAlert("Error", "Failed to open this ebook file.", "OK");
            }

            await Navigation.RemovePopupPageAsync(_loadingPopup);
        }

        private void OpenBook(BookInfo book) {
            SendBookToReader(book);
        }

        private async void DeleteBook(BookInfo book) {
            var deleteButton = "Delete";
            var deleteSyncButton = "Delete including all synchronizations";
            var confirm = await DisplayActionSheet("Delete book?", deleteButton, "No", deleteSyncButton);
            if (confirm == deleteButton || confirm == deleteSyncButton) {
                var card = Bookshelf.Children.FirstOrDefault(o => o.StyleId == book.Id);
                if (card != null) {
                    Bookshelf.Children.Remove(card);
                }
                _bookshelfService.RemoveById(book.Id);

                if (confirm == deleteSyncButton)
                {
                    var syncService = IocManager.Container.Resolve<ISyncService>();
                    syncService.DeleteBook(book.Id);
                }
            }
        }

        private async void SendBookToReader(BookInfo book) {
            var page = new ReaderPage();
            page.LoadBook(book);
            await Navigation.PushAsync(page);

            book.LastRead = DateTime.Now;
            _bookshelfService.SaveBook(book);
        }

        private void MyFloatButton_Clicked(object sender, EventArgs e) {
            _messageBus.Send(new AddBookClickedMessage());
        }
    }
}