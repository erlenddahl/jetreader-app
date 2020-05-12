using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.BookLoaders;
using JetReader.Books;
using JetReader.Config.CommandGrid;
using JetReader.Config.StatusPanel;
using JetReader.DependencyService;
using JetReader.Extensions;
using JetReader.Helpers;
using JetReader.Model.Format;
using JetReader.Model.Messages;
using JetReader.Model.WebViewMessages;
using JetReader.Page.Popups;
using JetReader.Page.Reader;
using JetReader.Page.Reader.Popups;
using JetReader.Provider;
using JetReader.Service;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json.Linq;
using Plugin.FilePicker.Abstractions;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ReaderPage : ContentPage {

        readonly IBookshelfService _bookshelfService;
        readonly IMessageBus _messageBus;
        readonly ISyncService _syncService;
        private readonly IBatteryProvider _batteryProvider;
        int _currentChapter;

        Ebook _ebook;
        BookInfo _bookshelfBook;

        bool _backgroundSync = true;
        Position _lastSavedPosition = null;
        Position _lastLoadedPosition = new Position();
        bool _syncPending = false;

        readonly QuickMenuPopup _quickPanel;
        private readonly IToastService _toastService;

        readonly LoadingPopup _loadingPopup = new LoadingPopup();

        public ReaderPage()
        {
            InitializeComponent();

            // ioc
            _bookshelfService = IocManager.Container.Resolve<IBookshelfService>();
            _messageBus = IocManager.Container.Resolve<IMessageBus>();
            _syncService = IocManager.Container.Resolve<ISyncService>();
            _batteryProvider = IocManager.Container.Resolve<IBatteryProvider>();
            _toastService = IocManager.Container.Resolve<IToastService>();
            IocManager.Container.Resolve<IMessageBus>().Subscribe<BatteryChangeMessage>((_) => { SetStatusPanelValue(StatusPanelItem.Battery, GetBatteryHtml()); }, nameof(ReaderPage));

            // webview events
            WebView.Messages.OnNextChapterRequest += _messages_OnNextChapterRequest;
            WebView.Messages.OnPrevChapterRequest += _messages_OnPrevChapterRequest;
            WebView.Messages.OnPageChange += Messages_OnPageChange;
            WebView.Messages.OnLinkClicked += Messages_OnLinkClicked;
            WebView.Messages.OnPanEvent += Messages_OnPanEvent;
            WebView.Messages.OnKeyStroke += Messages_OnKeyStroke;
            WebView.Messages.OnInteraction += Messages_OnInteraction;
            WebView.Messages.OnCommandRequest += Messages_OnCommandRequest;
            WebView.Messages.OnMessageReturned += Messages_OnMessageReturned;
            WebView.Messages.OnReadStats += MessagesOnReadStats;

            _quickPanel = new QuickMenuPopup();
            _chapterListPopup = new ChapterListPopup();
            _chapterListPopup.OnChapterClicked += ChapterList_OnChapterClicked;
            _chapterListPopup.OnBookmarkClicked += ChapterListPopupOnBookmarkClicked;

            ToolbarItems.Add(new ToolbarItem("Search", null, () => { }, ToolbarItemOrder.Secondary, 0));
            ToolbarItems.Add(new ToolbarItem("Sync", null, () => SynchronizeAll(), ToolbarItemOrder.Secondary, 0));
            ToolbarItems.Add(new ToolbarItem("Backup", null, () => DoBackup(), ToolbarItemOrder.Secondary, 0));
            ToolbarItems.Add(new ToolbarItem("Restore", null, () => DoRestore(), ToolbarItemOrder.Secondary, 0));
            ToolbarItems.Add(new ToolbarItem("Book info", null, () => new BookInfoPopup(_ebook).Show(), ToolbarItemOrder.Secondary, 0));
            ToolbarItems.Add(new ToolbarItem("Chapters/bookmarks", null, () => _chapterListPopup.Show(), ToolbarItemOrder.Secondary, 0));
        }

        private void ChapterListPopupOnBookmarkClicked(object sender, Bookmark e)
        {
            _chapterListPopup.Hide();
            OpenBookmark(e);
        }

        private void MessagesOnReadStats(object sender, ReadStats e)
        {
            _bookshelfBook.ReadStats.Save(e);
        }

        private bool _isFullscreen = false;
        private readonly ChapterListPopup _chapterListPopup;

        private void ToggleFullscreen(FullscreenRequestMessage msg)
        {
            if (!UserSettings.Reader.Fullscreen) return;

            _isFullscreen = msg.Fullscreen ?? !_isFullscreen;
            Device.BeginInvokeOnMainThread(() =>
            {
                NavigationPage.SetHasNavigationBar(this, !_isFullscreen);
            });
        }

        private object GetBatteryHtml()
        {
            return "<div class='battery-indicator'><div class='nub'></div><div class='battery'>" + _batteryProvider.RemainingChargePercent + "%</div></div>";
        }

        private async void Messages_OnMessageReturned(Message msg, string result)
        {
            if (msg.Action == "loadHtml")
            {
                await _loadingPopup.Hide();
            }
        }

        private async void Messages_OnCommandRequest(object sender, CommandRequest e)
        {
            switch (e.Command)
            {
                case GridCommand.ToggleFullscreen:
                    try
                    {
                        _messageBus.Send(new FullscreenRequestMessage(null));
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    break;
                case GridCommand.BookInfo:
                    await new BookInfoPopup(_ebook).Show();
                    break;
                case GridCommand.OpenQuickSettings:
                    await Navigation.PushPopupAsync(_quickPanel, false);
                    break;
                case GridCommand.Sync:
                    await SynchronizeAll();
                    break;
                case GridCommand.Backup:
                    await DoBackup();
                    break;
                case GridCommand.Restore:
                    await DoRestore();
                    break;
                case GridCommand.ShowChapters:
                    _chapterListPopup.Show();
                    break;
                case GridCommand.ShowBookmarks:
                    _chapterListPopup.Show();
                    break;
            }
        }

        private async Task DoBackup()
        {
            WebView.Messages.ShowProgressMessage("Performing backup ...");
            await Backup();
            WebView.Messages.ShowProgressMessage("Backup successful.", preset: ProgressMessagePreset.Success);
        }

        private async Task DoRestore()
        {
            WebView.Messages.ShowProgressMessage("Fetching restore list ...");
            var files = await _syncService.GetDatabaseRestorationList();
            if (files != null && files.Any())
                await new ItemPickerPopup("Pick backup file to restore:", files.Select(p => p.Split('/').Last().Split('.').First()).ToArray(), async (_, ix) =>
                {
                    WebView.Messages.ShowProgressMessage("Restoring backup ...");
                    try
                    {
                        var res = await _syncService.RestoreBackup(files[ix]);
                        if (res)
                            WebView.Messages.ShowProgressMessage("Backup restored.", preset: ProgressMessagePreset.Success);
                        else
                            WebView.Messages.ShowProgressMessage("Restore failed.", preset: ProgressMessagePreset.Failure);
                    }
                    catch (Exception ex)
                    {
                        WebView.Messages.ShowProgressMessage("Restore failed: " + ex.Message, preset: ProgressMessagePreset.Failure);
                    }
                }).Show();
            else
                WebView.Messages.ShowProgressMessage("No backup files found.", preset: ProgressMessagePreset.Failure);
        }

        private async Task SynchronizeAll()
        {
            WebView.Messages.ShowProgressMessage("Loading sync data from other devices ...");
            await LoadProgress();
            WebView.Messages.ShowProgressMessage("Saving sync data from this device ...");
            SaveProgress();
            WebView.Messages.ShowProgressMessage("Synchronizing bookmarks ...");
            SynchronizeBookmarks();
            WebView.Messages.ShowProgressMessage("Synchronization successful.", preset: ProgressMessagePreset.Success);
        }

        private async Task Backup()
        {
            await _syncService.BackupDatabase();
        }

        private void Messages_OnInteraction(object sender, JObject e)
        {
            if (PopupNavigation.Instance.PopupStack.Contains(_quickPanel)) return;
            _messageBus.Send(new FullscreenRequestMessage(true));
        }

        private void ChangeTheme(ChangeThemeMessage msg = null)
        {
            SendTheme(msg.Theme);
        }

        private void SubscribeMessages()
        {
            _messageBus.Subscribe<BookmarksChangedMessage>(p => _chapterListPopup.RefreshBookmarks(), nameof(ReaderPage));
            _messageBus.Subscribe<ChangeThemeMessage>(ChangeTheme, nameof(ReaderPage));
            _messageBus.Subscribe<ChangeMarginMessage>(ChangeMargin, nameof(ReaderPage));
            _messageBus.Subscribe<ChangeFontSizeMessage>(ChangeFontSize, nameof(ReaderPage));
            _messageBus.Subscribe<AppSleepMessage>(AppSleepSubscriber, nameof(ReaderPage));
            _messageBus.Subscribe<GoToPageMessage>(GoToPageHandler, nameof(ReaderPage));
            _messageBus.Subscribe<KeyStrokeMessage>(KeyStrokeHandler, nameof(ReaderPage));
            _messageBus.Subscribe<FullscreenRequestMessage>(ToggleFullscreen, nameof(ReaderPage));
        }

        private void UnSubscribeMessages() {
            _messageBus.UnSubscribe(nameof(ReaderPage));
        }

        private void Messages_OnPanEvent(object sender, PanEvent e) {

            if (UserSettings.Control.BrightnessChange == BrightnessChange.None)
                return;

            var totalWidth = (int)WebView.Width;
            var edge = totalWidth / 5;

            if ((UserSettings.Control.BrightnessChange != BrightnessChange.Left || e.StartX > edge) && (UserSettings.Control.BrightnessChange != BrightnessChange.Right || e.StartX < totalWidth - edge)) return;

            var brightness = UserSettings.Reader.Brightness - e.DiffY / WebView.Height;
            Debug.WriteLine(UserSettings.Reader.Brightness + ", " + e.DiffY + ", " + WebView.Height + ", " + (e.DiffY / WebView.Height) + ", " + brightness);
            brightness = ChangeBrightnessMessage.Validate(brightness);
            _messageBus.Send(new ChangeBrightnessMessage(brightness));

            if (e.IsFinal)
            {
                UserSettings.Reader.Brightness = brightness;
                Debug.WriteLine("Saved brightness: " + UserSettings.Reader.Brightness);
            }
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

            var chapter = _ebook.HtmlFiles.FirstOrDefault(p => p.Href.Contains(path));
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
            WebView.Messages.Send("pauseStatsTime", null);
            SaveProgress();
            _loadingPopup.Hide();
            _backgroundSync = false;
            _messageBus.Send(new FullscreenRequestMessage(null));
            UnSubscribeMessages();
        }

        protected override void OnAppearing() {
            base.OnAppearing();

            // If this page was added directly as a result of UserSettings.Reader.OpenPreviousBookOnLaunch,
            // add the home page "below" it on the stack to allow us to use the back button to "return" as usual.
            if (Navigation.NavigationStack.Count == 1)
                Navigation.InsertPageBefore(new HomePage(), this);

            _backgroundSync = true;
            SubscribeMessages();

            _messageBus.Send(new FullscreenRequestMessage(true));
            _messageBus.Send(new ChangeBrightnessMessage(UserSettings.Reader.Brightness));

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

            WebView.Messages.Send("resumeStatsTime", null);
        }
        public async Task LoadBook(FileData file)
        {
            try
            {
                await _loadingPopup.Show();
                var (book, _) = await _bookshelfService.AddBook(file);
                await LoadBook(book);
            }
            catch (Exception e)
            {
                var ext = string.Empty;
                if (!string.IsNullOrEmpty(file.FileName))
                    ext = Path.GetExtension(file.FileName);

                Analytics.TrackEvent("Failed to open book", new Dictionary<string, string>
                {
                    {"Extension", ext}
                });
                Crashes.TrackError(e, new Dictionary<string, string>
                {
                    {"Filename", file.FileName}
                });
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                await DisplayAlert("Error", "Failed to open this ebook file.", "OK");
            }

            await _loadingPopup.Hide();
        }

        public async Task LoadBook(BookInfo info)
        {
            await _loadingPopup.Show();

            _bookshelfBook = info;
            _bookshelfBook.LastRead = DateTime.Now;
            _bookshelfBook.ReadStats.OpenedBook();
            var loader = EbookFormatHelper.GetBookLoader(info.Format);
            _ebook = await loader.OpenBook(info);
            var position = _bookshelfBook.Position;

            Title = _ebook.Title + " - " + _ebook.Author;

            var chapter = _ebook.HtmlFiles.First();
            var positionInChapter = 0;

            if (position != null) {
                var loadedChapter = _ebook.HtmlFiles[position.Spine];
                if (loadedChapter != null) {
                    chapter = loadedChapter;
                    positionInChapter = position.SpinePosition;
                }
            }

            SendChapter(chapter, positionInChapter);

            _chapterListPopup.SetBook(_ebook, _bookshelfBook);
        }

        private void AppSleepSubscriber(AppSleepMessage msg) {
            if (Device.RuntimePlatform == Device.UWP && _backgroundSync) {
                SaveProgress();
            }
        }

        private void OpenBookmark(Bookmark bookmark)
        {
            var loadedChapter = _ebook.HtmlFiles[bookmark.Position.Spine];
            if (loadedChapter != null)
            {
                if (_currentChapter != bookmark.Position.Spine)
                {
                    SendChapter(loadedChapter, bookmark.Position.SpinePosition);
                }
                else
                {
                    _bookshelfBook.SpinePosition = bookmark.Position.SpinePosition;
                    GoToPosition(bookmark.Position.SpinePosition);
                }
            }
        }

        private void ChangeMargin(ChangeMarginMessage msg) {
            SetMargins(msg.Margins);
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
            _currentChapter = _ebook.HtmlFiles.IndexOf(chapter);
            _bookshelfBook.Spine = _currentChapter;

            var html = chapter.Content;
            var bookLoader = EbookFormatHelper.GetBookLoader(_bookshelfBook.Format);
            var preparedHtml = await bookLoader.PrepareHtml(html, _ebook, chapter);

            Device.BeginInvokeOnMainThread(() => {
                SendHtml(preparedHtml, chapter.Title, position, lastPage, marker);
                SetStatusPanelValues(new Dictionary<StatusPanelItem, object>() { { StatusPanelItem.BookTitle, _ebook.Title }, { StatusPanelItem.BookAuthor, _ebook.Author } });
            });

            await _ebook.Info.WaitForProcessingToFinish();

            Device.BeginInvokeOnMainThread(() =>
            {
                var (wordsBefore, wordsCurrent, wordsAfter) = _ebook.Info.GetWordCountsAround(chapter);
                var json = new JObject {{"wordsBefore", wordsBefore}, {"wordsCurrent", wordsCurrent}, {"wordsAfter", wordsAfter}};
                WebView.Messages.Send("setChapterInfo", json);
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

        private async Task LoadProgress() {
            if (_bookshelfBook == null) return;

            var syncPosition = await _syncService.LoadProgress(_bookshelfBook.Id);

            if (_syncPending || syncPosition?.Position == null || syncPosition.Position.Equals(_bookshelfBook.Position) || syncPosition.Position.Equals(_lastLoadedPosition) || syncPosition.D == UserSettings.Synchronization.DeviceName) return;

            var loadedChapter = _ebook.HtmlFiles[syncPosition.Position.Spine];

            if (loadedChapter == null) return;

            _lastLoadedPosition = new Position(syncPosition.Position);
            Device.BeginInvokeOnMainThread(async () => {
                _syncPending = true;
                var loadPosition = await DisplayAlert("Reading position at another device", $"Load reading position from the device {syncPosition.D}?", "Yes, load it", "No");
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

        private void ChapterList_OnChapterClicked(object sender, EbookChapter e)
        {
            _chapterListPopup.Hide();
            LoadChapter(e.Href);
        }

        private void Messages_OnPageChange(object sender, PageChange e) {
            _bookshelfBook.SpinePosition = e.Position;
            _bookshelfService.SaveBook(_bookshelfBook);
            _messageBus.Send(new PageChangeMessage { CurrentPage = e.CurrentPage, TotalPages = e.TotalPages, Position = e.Position });
        }

        private void _messages_OnPrevChapterRequest(object sender, PrevChapterRequest e) {
            if (_currentChapter > 0) {
                SendChapter(_ebook.HtmlFiles[_currentChapter - 1], lastPage: true);
            }

            _bookshelfBook.FinishedReading = null;
            _bookshelfService.SaveBook(_bookshelfBook);
        }

        private void _messages_OnNextChapterRequest(object sender, NextChapterRequest e) {
            if (_currentChapter < _ebook.HtmlFiles.Count - 1) {
                SendChapter(_ebook.HtmlFiles[_currentChapter + 1]);
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
        #endregion

        #region webview messages
        private void InitWebView(int width, int height) {
            var json = new
            {
                Width = width,
                Height = height,
                UserSettings.Reader.FontSize,
                UserSettings.Reader.Margins,
                UserSettings.Reader.ScrollSpeed,
                UserSettings.Control.DoubleSwipe,
                UserSettings.Reader.Theme,
                Commands = UserSettings.Control.CommandGrid.ToJson(),
                StatusPanelData = new
                {
                    PanelDefinition = StatusPanelConfig.DefaultPanelDefinitions[0].ToJson(),
                    Values = new
                    {
                        Clock = DateTime.Now.ToString("HH:mm"),
                        Battery = GetBatteryHtml()
                    }
                }
            };

            WebView.Messages.Send("init", json);
        }

        private void SetStatusPanelValue(StatusPanelItem key, object value)
        {
            var d = new Dictionary<StatusPanelItem, object> {{key, value}};
            SetStatusPanelValues(d);
        }

        private void SetStatusPanelValues(Dictionary<StatusPanelItem, object> keyValues)
        {
            WebView.Messages.Send("setStatusPanelData", JObject.FromObject(keyValues));
        }

        private void RefreshWebViewSize()
        {
            ResizeWebView((int)WebView.Width, (int)WebView.Height);
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

        private void SetFontSize(double fontSize) {
            var json = new {
                FontSize = fontSize
            };

            WebView.Messages.Send("changeFontSize", json);
        }

        private void SetMargins(Margin margins) {
            WebView.Messages.Send("changeMargin", margins);
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

        private void SendTheme(Theme theme)
        {
            WebView.Messages.Send("setTheme", theme);
        }
        #endregion

    }
}