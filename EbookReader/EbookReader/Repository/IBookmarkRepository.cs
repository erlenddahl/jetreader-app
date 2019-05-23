using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Books;

namespace EbookReader.Repository
{
    public interface IBookmarkRepository {
        Task<List<Bookmark>> GetBookmarksByBookIdAsync(string bookId);
        Task<int> DeleteBookmarkAsync(Bookmark book);
        Task<int> SaveBookmarkAsync(Bookmark item);
    }
}
