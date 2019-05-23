using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Books;
using EbookReader.Provider;
using EbookReader.Repository;

namespace EbookReader.Service
{
    public class BookmarkService : IBookmarkService {
        readonly IBookmarkRepository _bookmarkRepository;
        IBookRepository _bookRepository;
        readonly ISyncService _syncService;

        public BookmarkService(IBookmarkRepository bookmarkRepository, IBookRepository bookRepository, ISyncService syncService) {
            _bookmarkRepository = bookmarkRepository;
            _bookRepository = bookRepository;
            _syncService = syncService;
        }

        public void CreateBookmark(string name, string bookId, Position position) {
            var bookmark = new Bookmark {
                Id = BookmarkIdProvider.Id,
                Name = name,
                Position = new Position(position),
                BookId = bookId,
            };

            SaveBookmark(bookmark);
            _syncService.SaveBookmark(bookId, bookmark);
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
