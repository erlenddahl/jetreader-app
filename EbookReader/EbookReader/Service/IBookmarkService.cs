using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Model.Bookshelf;

namespace EbookReader.Service {
    public interface IBookmarkService {
        void DeleteBookmark(Bookmark bookmark, string bookId);
        Task<List<Bookmark>> LoadBookmarksByBookId(string bookId);
        void CreateBookmark(string name, string bookId, Position position);
        void SaveBookmark(Bookmark bookmark);
    }
}
