using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using EbookReader.Service;
using EbookReader.Books;

namespace EbookReader.Repository {
    public class BookRepository : IBookRepository {
        readonly SQLiteAsyncConnection _connection;

        public BookRepository(IDatabaseService databaseService) {
            _connection = databaseService.Connection;
        }

        public Task<List<BookInfo>> GetAllBooksAsync()
        {
            return _connection.Table<BookInfo>().OrderByDescending(p => p.LastRead).ToListAsync();
        }

        public Task<BookInfo> GetBookByIdAsync(string id) {
            return _connection.Table<BookInfo>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public Task<int> DeleteBookAsync(BookInfo book) {
            return _connection.DeleteAsync(book);
        }

        public Task<int> SaveBookAsync(BookInfo item) {
            return _connection.InsertOrReplaceAsync(item);
        }

        public Task<int> DeleteAllBooksAsync()
        {
            return _connection.Table<BookInfo>().DeleteAsync(p => true);
        }

        public Task<BookInfo> GetMostRecentBook()
        {
            return _connection.Table<BookInfo>().OrderByDescending(p => p.LastRead).FirstOrDefaultAsync();
        }
    }
}
