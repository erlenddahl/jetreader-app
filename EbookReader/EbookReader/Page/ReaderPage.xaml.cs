using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EbookReader.BookLoaders;
using EbookReader.DependencyService;
using EbookReader.Helpers;
using EbookReader.Model.Bookshelf;
using EbookReader.Model.Format;
using EbookReader.Model.Messages;
using EbookReader.Model.WebViewMessages;
using EbookReader.Page.Reader;
using EbookReader.Provider;
using EbookReader.Service;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReaderPage : ContentPage {

        IAssetsManager _assetsManager;
        readonly IBookshelfService _bookshelfService;
        readonly IMessageBus _messageBus;
        readonly ISyncService _syncService;
        readonly IBookmarkService _bookmarkService;
        private readonly IBatteryProvider _batteryProvider;
        int _currentChapter;

        Book _bookshelfBook;
        Ebook _ebook;

        bool _resizeFirstRun = true;
        bool _resizeTimerRunning = false;
        int? _resizeTimerWidth;
        int? _resizeTimerHeight;

        bool _backgroundSync = true;
        Position _lastSavedPosition = null;
        Position _lastLoadedPosition = new Position();
        bool _syncPending = false;

        QuickPanel _quickPanel;
        private IToastService _toastService;

        public ReaderPage() {
            InitializeComponent();

            // ioc
            _assetsManager = IocManager.Container.Resolve<IAssetsManager>();
            _bookshelfService = IocManager.Container.Resolve<IBookshelfService>();
            _messageBus = IocManager.Container.Resolve<IMessageBus>();
            _syncService = IocManager.Container.Resolve<ISyncService>();
            _bookmarkService = IocManager.Container.Resolve<IBookmarkService>();
            _batteryProvider = IocManager.Container.Resolve<IBatteryProvider>();
            _toastService = IocManager.Container.Resolve<IToastService>();
            IocManager.Container.Resolve<IMessageBus>().Subscribe<BatteryChangeMessage>((_)=> { SetStatusPanelValue("battery", GetBatteryHtml()); });

            // webview events
            WebView.Messages.OnNextChapterRequest += _messages_OnNextChapterRequest;
            WebView.Messages.OnPrevChapterRequest += _messages_OnPrevChapterRequest;
            WebView.Messages.OnOpenQuickPanelRequest += _messages_OnOpenQuickPanelRequest;
            WebView.Messages.OnPageChange += Messages_OnPageChange;
            WebView.Messages.OnLinkClicked += Messages_OnLinkClicked;
            WebView.Messages.OnPanEvent += Messages_OnPanEvent;
            WebView.Messages.OnKeyStroke += Messages_OnKeyStroke;
            WebView.Messages.OnInteraction += Messages_OnInteraction;
            WebView.Messages.OnCommandRequest += Messages_OnCommandRequest;

            var quickPanelPosition = new Rectangle(0, 0, 1, 0.75);

            if (Device.RuntimePlatform == Device.UWP) {
                quickPanelPosition = new Rectangle(0, 0, 0.33, 1);
            }
            
            _quickPanel = new QuickPanel();
            _quickPanel.PanelContent.OnChapterChange += PanelContent_OnChapterChange;

            NavigationPage.SetHasNavigationBar(this, false);
            _messageBus.Send(new FullscreenRequestMessage(true));

            ChangeTheme();
        }

        private object GetBatteryHtml()
        {
            return "<div class='battery-indicator'><div class='nub'></div><div class='battery'>" + _batteryProvider.RemainingChargePercent + "%</div></div>";
        }

        private void Messages_OnCommandRequest(object sender, CommandRequest e)
        {
            switch (e.Command)
            {
                case "toggleFullscreen":
                    try
                    {
                        _messageBus.Send(new FullscreenRequestMessage(null));
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    break;
            }
        }

        private void Messages_OnInteraction(object sender, JObject e)
        {
            if (PopupNavigation.Instance.PopupStack.Contains(_quickPanel)) return;
            _messageBus.Send(new FullscreenRequestMessage(true));
        }

        private void ChangeTheme(ChangeThemeMessage msg = null)
        {
            if (UserSettings.Reader.NightMode)
            {
                BackgroundColor = Color.FromRgb(24, 24, 25);
            }
            else
            {
                BackgroundColor = Color.FromRgb(0, 0, 0);
            }

            SendNightMode(UserSettings.Reader.NightMode);
        }

        private void SubscribeMessages()
        {
            _messageBus.Subscribe<ChangeThemeMessage>(ChangeTheme, new[] { nameof(ReaderPage) });
            _messageBus.Subscribe<ChangeMarginMessage>(ChangeMargin, new[] { nameof(ReaderPage) });
            _messageBus.Subscribe<ChangeFontSizeMessage>(ChangeFontSize, new[] { nameof(ReaderPage) });
            _messageBus.Subscribe<AppSleepMessage>(AppSleepSubscriber, new[] { nameof(ReaderPage) });
            _messageBus.Subscribe<AddBookmarkMessage>(AddBookmark, new[] { nameof(ReaderPage) });
            _messageBus.Subscribe<OpenBookmarkMessage>(OpenBookmark, new[] { nameof(ReaderPage) });
            _messageBus.Subscribe<DeleteBookmarkMessage>(DeleteBookmark, new[] { nameof(ReaderPage) });
            _messageBus.Subscribe<ChangedBookmarkNameMessage>(ChangedBookmarkName, new[] { nameof(ReaderPage) });
            _messageBus.Subscribe<GoToPageMessage>(GoToPageHandler, new[] { nameof(ReaderPage) });
            _messageBus.Subscribe<KeyStrokeMessage>(KeyStrokeHandler, new[] { nameof(ReaderPage) });
        }

        private void UnSubscribeMessages() {
            _messageBus.UnSubscribe(nameof(ReaderPage));
        }

        private void Messages_OnPanEvent(object sender, PanEvent e) {

            if (UserSettings.Control.BrightnessChange == BrightnessChange.None) {
                return;
            }

            var totalWidth = (int)WebView.Width;
            var edge = totalWidth / 5;

            if ((UserSettings.Control.BrightnessChange != BrightnessChange.Left || e.X > edge) && (UserSettings.Control.BrightnessChange != BrightnessChange.Right || e.X < totalWidth - edge)) return;

            var brightness = 1 - ((float)e.Y / ((int)WebView.Height + (2 * UserSettings.Reader.Margin)));
            _messageBus.Send(new ChangesBrightnessMessage { Brightness = brightness });
        }

        private void Messages_OnLinkClicked(object sender, LinkClicked e)
        {
            if (string.IsNullOrEmpty(e.Href)) return;

            if(!e.Href.Contains("://"))
                LoadChapter(e.Href);
            else
            {
                try
                {
                    var uri = new Uri(e.Href);
                    Device.OpenUri(uri);
                }
                catch (Exception ex)
                {
                    _toastService.Show("Failed to open url: " + ex.Message);
                }
            }
        }

        private void LoadChapter(string chapterPath)
        {
            if (string.IsNullOrEmpty(chapterPath)) return;

            var parts = chapterPath.Split('#');
            var path = parts.FirstOrDefault();
            var hash = parts.Skip(1).FirstOrDefault();

            var chapter = _ebook.Chapters.FirstOrDefault(p => p.Href.Contains(path));
            if (chapter != null)
                SendChapter(chapter, marker: hash);
            else
                _toastService.Show("Failed to open chapter.");
        }

        private void Messages_OnKeyStroke(object sender, KeyStroke e) {
            _messageBus.Send(KeyStrokeMessage.FromKeyCode(e.KeyCode));
        }

        protected override void OnDisappearing() {
            base.OnDisappearing();
            SaveProgress();
            _backgroundSync = false;
            UnSubscribeMessages();
        }

        protected override void OnAppearing() {
            base.OnAppearing();
            _backgroundSync = true;
            _messageBus.Send(new FullscreenRequestMessage(true));
            SubscribeMessages();

            Task.Run(() => {
                LoadProgress();
                SynchronizeBookmarks();
            });

            Device.StartTimer(new TimeSpan(0, 1, 0), () => {
                if (!_backgroundSync) return _backgroundSync;

                LoadProgress();
                SaveProgress();
                SynchronizeBookmarks();

                return _backgroundSync;
            });
        }

        public async void LoadBook(Book book) {
            _bookshelfBook = book;
            _ebook = await EbookFormatHelper.GetBookLoader(book.Format).OpenBook(book.BookLocation);
            var position = _bookshelfBook.Position;

            Title = _ebook.Title + " - " + _ebook.Author;
            _quickPanel.PanelContent.SetNavigation(_ebook.Chapters);
            RefreshBookmarks();

            var chapter = _ebook.Chapters.First();
            var positionInChapter = 0;

            if (position != null) {
                var loadedChapter = _ebook.Chapters[position.Spine];
                if (loadedChapter != null) {
                    chapter = loadedChapter;
                    positionInChapter = position.SpinePosition;
                }
            }

            SendChapter(chapter, positionInChapter);
        }

        private void AppSleepSubscriber(AppSleepMessage msg) {
            if (Device.RuntimePlatform == Device.UWP && _backgroundSync) {
                SaveProgress();
            }
        }

        private void AddBookmark(AddBookmarkMessage msg) {
            _bookmarkService.CreateBookmark(DateTime.Now.ToString(), _bookshelfBook.Id, _bookshelfBook.Position);

            RefreshBookmarks();
        }

        private void DeleteBookmark(DeleteBookmarkMessage msg) {
            _bookmarkService.DeleteBookmark(msg.Bookmark, _bookshelfBook.Id);

            RefreshBookmarks();
        }

        public void ChangedBookmarkName(ChangedBookmarkNameMessage msg) {
            _bookmarkService.SaveBookmark(msg.Bookmark);
            _syncService.SaveBookmark(_bookshelfBook.Id, msg.Bookmark);
            RefreshBookmarks();
        }

        private async void RefreshBookmarks() {
            var bookmarks = await _bookmarkService.LoadBookmarksByBookId(_bookshelfBook.Id);
            _quickPanel.PanelBookmarks.SetBookmarks(bookmarks);
        }

        private void OpenBookmark(OpenBookmarkMessage msg) {
            var loadedChapter = _ebook.Chapters[msg.Bookmark.Position.Spine];
            if (loadedChapter != null) {
                if (_currentChapter != msg.Bookmark.Position.Spine) {
                    SendChapter(loadedChapter, position: msg.Bookmark.Position.SpinePosition);
                } else {
                    _bookshelfBook.SpinePosition = msg.Bookmark.Position.SpinePosition;
                    GoToPosition(msg.Bookmark.Position.SpinePosition);
                }
            }
        }

        private void ChangeMargin(ChangeMarginMessage msg) {
            SetMargin(msg.Margin);
        }

        private void ChangeFontSize(ChangeFontSizeMessage msg) {
            SetFontSize(msg.FontSize);
        }

        private void GoToPageHandler(GoToPageMessage msg) {
            SendGoToPage(msg.Page, msg.Next, msg.Previous);
        }

        private void KeyStrokeHandler(KeyStrokeMessage msg) {
            switch (msg.Key) {
                case Key.Space:
                case Key.ArrowRight:
                case Key.ArrowDown:
                    SendGoToPage(0, true, false);
                    break;
                case Key.ArrowLeft:
                case Key.ArrowUp:
                    SendGoToPage(0, false, true);
                    break;
            }
        }

        private async void SendChapter(EbookChapter chapter, int position = 0, bool lastPage = false, string marker = "") {
            _currentChapter = _ebook.Chapters.IndexOf(chapter);
            _bookshelfBook.Spine = _currentChapter;

            var html = chapter.Content;
            var bookLoader = EbookFormatHelper.GetBookLoader(_bookshelfBook.Format);
            var preparedHtml = await bookLoader.PrepareHtml(html, _ebook, chapter);

            Device.BeginInvokeOnMainThread(() => {
                SendHtml(preparedHtml, chapter.Title, position, lastPage, marker);
            });

        }

        #region sync
        private void SaveProgress() {
            if (_bookshelfBook == null) return;
            _bookshelfService.SaveBook(_bookshelfBook);
            if (_bookshelfBook.Position.Equals(_lastSavedPosition)) return;
            _lastSavedPosition = new Position(_bookshelfBook.Position);
            _syncService.SaveProgress(_bookshelfBook.Id, _bookshelfBook.Position);
        }

        private async void LoadProgress() {
            if (_bookshelfBook == null) return;

            var syncPosition = await _syncService.LoadProgress(_bookshelfBook.Id);

            if (_syncPending || syncPosition == null || syncPosition.Position == null || syncPosition.Position.Equals(_bookshelfBook.Position) || syncPosition.Position.Equals(_lastLoadedPosition) || syncPosition.D == UserSettings.Synchronization.DeviceName) return;

            var loadedChapter = _ebook.Chapters[syncPosition.Position.Spine];

            if (loadedChapter == null) return;

            _lastLoadedPosition = new Position(syncPosition.Position);
            Device.BeginInvokeOnMainThread(async () => {
                _syncPending = true;
                var loadPosition = await DisplayAlert("Reading position at the another device", $"Load reading position from the device {syncPosition.D}?", "Yes, load it", "No");
                if (loadPosition) {
                    if (_currentChapter != syncPosition.Position.Spine) {
                        SendChapter(loadedChapter, position: syncPosition.Position.SpinePosition);
                    } else {
                        _bookshelfBook.SpinePosition = syncPosition.Position.SpinePosition;
                        _bookshelfService.SaveBook(_bookshelfBook);
                        GoToPosition(syncPosition.Position.SpinePosition);
                    }
                }
                _syncPending = false;
            });
        }

        private void SynchronizeBookmarks() {
            if (_bookshelfBook == null) return;
            _syncService.SynchronizeBookmarks(_bookshelfBook);
        }
        #endregion

        #region events

        private void PanelContent_OnChapterChange(object sender, EbookChapter e)
        {
            LoadChapter(e.Href);
        }

        private void Messages_OnPageChange(object sender, PageChange e) {
            _bookshelfBook.SpinePosition = e.Position;
            _bookshelfService.SaveBook(_bookshelfBook);
            _messageBus.Send(new PageChangeMessage { CurrentPage = e.CurrentPage, TotalPages = e.TotalPages, Position = e.Position });
        }

        private void _messages_OnOpenQuickPanelRequest(object sender, OpenQuickPanelRequest e) {
            PopupNavigation.Instance.PushAsync(_quickPanel, false);
        }

        private void _messages_OnPrevChapterRequest(object sender, PrevChapterRequest e) {
            if (_currentChapter > 0) {
                SendChapter(_ebook.Chapters[_currentChapter - 1], lastPage: true);
            }

            _bookshelfBook.FinishedReading = null;
            _bookshelfService.SaveBook(_bookshelfBook);
        }

        private void _messages_OnNextChapterRequest(object sender, NextChapterRequest e) {
            if (_currentChapter < _ebook.Chapters.Count - 1) {
                SendChapter(_ebook.Chapters[_currentChapter + 1]);
                _bookshelfBook.FinishedReading = null;
            } else {
                _bookshelfBook.FinishedReading = DateTime.UtcNow;
            }

            _bookshelfService.SaveBook(_bookshelfBook);
        }

        private void WebView_OnContentLoaded(object sender, EventArgs e) {
            InitWebView(
                (int)WebView.Width,
                (int)WebView.Height
            );
        }

        private void WebView_SizeChanged(object sender, EventArgs e) {

            if (_resizeFirstRun) {
                _resizeFirstRun = false;
                return;
            }

            _resizeTimerWidth = (int)WebView.Width;
            _resizeTimerHeight = (int)WebView.Height;

            if (_resizeTimerRunning) return;

            _resizeTimerRunning = true;
            Device.StartTimer(new TimeSpan(0, 0, 0, 0, 500), () => {

                if (_resizeTimerWidth.HasValue && _resizeTimerHeight.HasValue) {
                    ResizeWebView(_resizeTimerWidth.Value, _resizeTimerHeight.Value);
                }

                _resizeTimerRunning = false;

                return false;
            });
        }
        #endregion

        #region webview messages
        private void InitWebView(int width, int height) {
            var json = new
            {
                Width = width,
                Height = height,
                UserSettings.Reader.Margin,
                UserSettings.Reader.FontSize,
                UserSettings.Reader.ScrollSpeed,
                UserSettings.Control.ClickEverywhere,
                UserSettings.Control.DoubleSwipe,
                UserSettings.Reader.NightMode,
                StatusPanelData = new
                {
                    PanelDefinition = new[] {
                        new[] { "battery", "clock" },
                        new[] { "chapter", "chapterProgress" },
                        new[] { "bookProgressPercentage" }
                    },
                    Values = new
                    {
                        clock = DateTime.Now.ToString("HH:mm"),
                        battery = GetBatteryHtml()
                    }
                }
            };

            WebView.Messages.Send("init", json);
        }

        private void SetStatusPanelValue(string key, object value)
        {
            var d = new Dictionary<string, object>();
            d.Add(key, value);
            SetStatusPanelValues(d);
        }

        private void SetStatusPanelValues(Dictionary<string, object> keyValues)
        {
            WebView.Messages.Send("setStatusPanelData", JObject.FromObject(keyValues));
        }

        private void ResizeWebView(int width, int height) {
            var json = new {
                Width = width,
                Height = height
            };

            WebView.Messages.Send("resize", json);
        }

        private void SendHtml(string html, string title, int position = 0, bool lastPage = false, string marker = "") {
            var json = new {
                Html = html,
                Title = title,
                Position = position,
                LastPage = lastPage,
                Marker = marker,
            };

            WebView.Messages.Send("loadHtml", json);
        }

        private void SetFontSize(int fontSize) {
            var json = new {
                FontSize = fontSize
            };

            WebView.Messages.Send("changeFontSize", json);
        }

        private void SetMargin(int margin) {
            var json = new {
                Margin = margin
            };

            WebView.Messages.Send("changeMargin", json);
        }

        private void GoToPosition(int position) {
            var json = new {
                Position = position,
            };

            WebView.Messages.Send("goToPosition", json);
        }

        private void SendGoToPage(int page, bool next, bool previous)
        {
            var json = new
            {
                Page = page,
                Next = next,
                Previous = previous,
            };

            WebView.Messages.Send("goToPage", json);
        }

        private void SendNightMode(bool nightmode)
        {
            var json = new
            {
                NightMode = nightmode
            };

            WebView.Messages.Send("setNightMode", json);
        }
        #endregion
    }
}