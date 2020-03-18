using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Books;

namespace JetReader.Service
{
    public interface IBookmarkService {
        void DeleteBookmark(Bookmark bookmark, string bookId);
        Task<List<Bookmark>> LoadBookmarksByBookId(string bookId);
        Bookmark CreateBookmark(string name, string bookId, Position position);
        void SaveBookmark(Bookmark bookmark);
    }
}
