using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Model.Bookshelf;
using EbookReader.Model.Sync;

namespace EbookReader.Service {
    public interface ISyncService {
        void SaveProgress(string bookId, Position position);
        Task<Progress> LoadProgress(string bookId);
        void DeleteBook(string bookId);
        void SaveBookmark(string bookId, Model.Bookshelf.Bookmark bookmark);
        void SynchronizeBookmarks(Book book);
    }
}
