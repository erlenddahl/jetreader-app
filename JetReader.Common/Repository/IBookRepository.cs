using JetReader.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Repository {
    public interface IBookRepository {
        Task<List<BookInfo>> GetAllBooksAsync();
        Task<BookInfo> GetBookByIdAsync(string id);
        Task<int> DeleteBookAsync(BookInfo book);
        Task<int> SaveBookAsync(BookInfo item);
        Task<int> DeleteAllBooksAsync();
        Task<BookInfo> GetMostRecentBook();
    }
}
