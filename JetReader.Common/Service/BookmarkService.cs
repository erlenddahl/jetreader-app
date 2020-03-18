using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Books;
using JetReader.Provider;
using JetReader.Repository;

namespace JetReader.Service
{
    public class BookmarkService : IBookmarkService {
        readonly IBookmarkRepository _bookmarkRepository;
        readonly ISyncService _syncService;

        public BookmarkService(IBookmarkRepository bookmarkRepository, ISyncService syncService) {
            _bookmarkRepository = bookmarkRepository;
            _syncService = syncService;
        }

        public Bookmark CreateBookmark(string name, string bookId, Position position) {
            var bookmark = new Bookmark {
                Id = BookmarkIdProvider.Id,
                Name = name,
                Position = new Position(position),
                BookId = bookId,
            };

            SaveBookmark(bookmark);
            _syncService.SaveBookmark(bookId, bookmark);

            return bookmark;
        }

        public async void DeleteBookmark(Bookmark bookmark, string bookId) {
            bookmark.Deleted = true;
            bookmark.Name = string.Empty;
            bookmark.Position = new Position();

            await _bookmarkRepository.SaveBookmarkAsync(bookmark);
            _syncService.SaveBookmark(bookId, bookmark);
        }

        public async Task<List<Bookmark>> LoadBookmarksByBookId(string bookId) {
            return await _bookmarkRepository.GetBookmarksByBookIdAsync(bookId);
        }

        public async void SaveBookmark(Bookmark bookmark) {
            bookmark.LastChange = DateTime.UtcNow;
            await _bookmarkRepository.SaveBookmarkAsync(bookmark);
        }
    }
}
