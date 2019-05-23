using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Books;
using EbookReader.Model.Sync;

namespace EbookReader.Service
{
    public interface ISyncService {
        void SaveProgress(string bookId, Position position);
        Task<Progress> LoadProgress(string bookId);
        void DeleteBook(string bookId);
        void SaveBookmark(string bookId, Books.Bookmark bookmark);
        void SynchronizeBookmarks(BookInfo book);
    }
}
