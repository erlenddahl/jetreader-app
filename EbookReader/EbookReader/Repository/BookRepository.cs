using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using EbookReader.Model.Bookshelf;
using EbookReader.Service;

namespace EbookReader.Repository {
    public class BookRepository : IBookRepository {
        readonly SQLiteAsyncConnection _connection;

        public BookRepository(IDatabaseService databaseService) {
            _connection = databaseService.Connection;
        }

        public Task<List<Book>> GetAllBooksAsync() {
            return _connection.Table<Book>().ToListAsync();
        }

        public Task<Book> GetBookByIdAsync(string id) {
            return _connection.Table<Book>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public Task<int> DeleteBookAsync(Book book) {
            return _connection.DeleteAsync(book);
        }

        public Task<int> SaveBookAsync(Book item) {
            return _connection.InsertOrReplaceAsync(item);
        }
    }
}
