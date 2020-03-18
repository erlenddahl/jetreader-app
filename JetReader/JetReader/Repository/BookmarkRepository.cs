using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Books;
using JetReader.Service;
using SQLite;

namespace JetReader.Repository
{
    public class BookmarkRepository : IBookmarkRepository {
        readonly SQLiteAsyncConnection _connection;

        public BookmarkRepository(IDatabaseService databaseService) {
            _connection = databaseService.Connection;
        }

        public Task<List<Bookmark>> GetBookmarksByBookIdAsync(string bookId) {
            return _connection.Table<Bookmark>().Where(o => o.BookId == bookId).ToListAsync();
        }
        
        public Task<int> DeleteBookmarkAsync(Bookmark bookmark) {
            return _connection.DeleteAsync(bookmark);
        }

        public Task<int> SaveBookmarkAsync(Bookmark bookmark) {
            return _connection.InsertOrReplaceAsync(bookmark);
        }
    }
}