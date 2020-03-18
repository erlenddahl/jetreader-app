using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.Books;
using JetReader.DependencyService;
using JetReader.Model.Messages;
using JetReader.Model.Sync;
using JetReader.Provider;
using Newtonsoft.Json;
using Plugin.Connectivity;

namespace JetReader.Service
{
    public class SyncService : ISyncService {

        const string ProgressNode = "progress";
        const string BookmarksNode = "bookmarks";
        const string BookmarksLastChangeNode = "bookmarkslastchange";

        readonly ICloudStorageService _cloudStorageService;
        readonly IBookshelfService _bookshelfService;
        readonly IMessageBus _messageBus;

        public SyncService(IBookshelfService bookshelfService, IMessageBus messageBus) {
            _bookshelfService = bookshelfService;
            _messageBus = messageBus;

            var service = UserSettings.Synchronization.Enabled ? UserSettings.Synchronization.Service : SynchronizationServicesProvider.Dumb;
            _cloudStorageService = IocManager.Container.ResolveKeyed<ICloudStorageService>(service);
        }

        public async Task<Progress> LoadProgress(string bookId) {

            if (!CanSync()) return null;

            var path = PathGenerator(bookId, ProgressNode);

            return await _cloudStorageService.LoadJson<Progress>(path);
        }

        public void SaveProgress(string bookId, Position position)
        {

            if (!CanSync()) return;

            var progress = new Progress
            {
                DeviceName = UserSettings.Synchronization.DeviceName,
                Position = position,
            };

            var path = PathGenerator(bookId, ProgressNode);

            _cloudStorageService.SaveJson(progress, path);
        }

        public async Task BackupDatabase()
        {
            if (!CanSync()) return;

            var filePath = IocManager.Container.Resolve<IFileHelper>().GetLocalFilePath(AppSettings.Bookshelft.SqlLiteFilename);
            await _cloudStorageService.BackupFile(filePath, new[] {"backup", DateTime.Now.ToString("yyyy-MM-dd") + " (" + UserSettings.Synchronization.DeviceName + ").backup"});
        }

        public async Task<List<string>> GetDatabaseRestorationList()
        {
            if (!CanSync()) return null;
            return await _cloudStorageService.GetFileList("/backup/", p => p.EndsWith(".backup"));
        }

        public async Task<bool> RestoreBackup(string fromPath)
        {
            if (!CanSync()) return false;
            var toPath = IocManager.Container.Resolve<IFileHelper>().GetLocalFilePath(AppSettings.Bookshelft.SqlLiteFilename);
            return await _cloudStorageService.RestoreFile(toPath, fromPath);
        }

        public void DeleteBook(string bookId) {

            if (!CanSync()) return;

            _cloudStorageService.DeleteNode(PathGenerator(bookId));
        }

        public void SaveBookmark(string bookId, Books.Bookmark bookmark) {

            if (!CanSync()) return;

            var syncBookmark = Model.Sync.Bookmark.FromDbBookmark(bookmark);

            var path = PathGenerator(bookId, BookmarksNode, bookmark.Id.ToString());

            _cloudStorageService.SaveJson(syncBookmark, path);
            SaveBookmarksLastChange(bookId);
        }

        public async void SynchronizeBookmarks(BookInfo book) {

            if (!CanSync()) return;

            var bookmarkService = IocManager.Container.Resolve<IBookmarkService>();

            var data = await _cloudStorageService.LoadJson<DateTime?>(PathGenerator(book.Id, BookmarksLastChangeNode));

            if (data.HasValue && book.BookmarksSyncLastChange.HasValue && book.BookmarksSyncLastChange.Value >= data.Value) return;

            var cloudBookmarks = await _cloudStorageService.LoadJsonList<Model.Sync.Bookmark>(PathGenerator(book.Id, BookmarksNode));
            var deviceBookmarks = await bookmarkService.LoadBookmarksByBookId(book.Id);

            var change = false;

            foreach (var cloudBookmark in cloudBookmarks) {
                var deviceBookmark = deviceBookmarks.FirstOrDefault(o => o.Id == cloudBookmark.Id);
                if (deviceBookmark == null && !cloudBookmark.Deleted) {
                    deviceBookmark = cloudBookmark.ToDbBookmark(book.Id);

                    deviceBookmarks.Add(deviceBookmark);

                    change = true;
                    bookmarkService.SaveBookmark(deviceBookmark);
                } else if (deviceBookmark != null && deviceBookmark.LastChange < cloudBookmark.LastChange) {
                    deviceBookmark.Name = cloudBookmark.Name;
                    deviceBookmark.Deleted = cloudBookmark.Deleted;
                    deviceBookmark.LastChange = DateTime.UtcNow;

                    change = true;
                    bookmarkService.SaveBookmark(deviceBookmark);
                }
            }

            var cloudMissingBookmarks = deviceBookmarks.Select(o => o.Id).Except(cloudBookmarks.Select(o => o.Id)).ToList();

            if (cloudMissingBookmarks.Any()) {
                change = true;
            }

            foreach (var deviceBookmark in deviceBookmarks.Where(o => cloudMissingBookmarks.Contains(o.Id))) {
                SaveBookmark(book.Id, deviceBookmark);
            }

            _bookshelfService.SaveBook(book);

            if (change) {
                SaveBookmarksLastChange(book.Id);
            }

            var bookmarks = await bookmarkService.LoadBookmarksByBookId(book.Id);
            _messageBus.Send(new BookmarksChangedMessage {
                Bookmarks = bookmarks
            });

        }

        private async void SaveBookmarksLastChange(string bookId) {
            var datetime = DateTime.UtcNow;
            _cloudStorageService.SaveJson(datetime, PathGenerator(bookId, BookmarksLastChangeNode));
            var book = await _bookshelfService.LoadBookById(bookId);
            book.BookmarksSyncLastChange = datetime;
            _bookshelfService.SaveBook(book);
        }

        private string[] PathGenerator(string bookId, params string[] nodes) {
            return new[] { "data", bookId }.Union(nodes).Where(o => !string.IsNullOrEmpty(o)).ToArray();
        }

        private bool CanSync() {
            if (!UserSettings.Synchronization.Enabled) return false;
            if (!CrossConnectivity.Current.IsConnected) return false;
            if (UserSettings.Synchronization.OnlyWifi &&
                !(CrossConnectivity.Current.ConnectionTypes.Contains(Plugin.Connectivity.Abstractions.ConnectionType.WiFi) ||
                  CrossConnectivity.Current.ConnectionTypes.Contains(Plugin.Connectivity.Abstractions.ConnectionType.Desktop))
                ) return false;
            if (!_cloudStorageService.IsConnected()) return false;

            return true;
        }
    }
}
