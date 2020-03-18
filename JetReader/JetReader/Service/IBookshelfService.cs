using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Books;
using Plugin.FilePicker.Abstractions;

namespace JetReader.Service {
    public interface IBookshelfService {
        Task<(BookInfo book, bool isNew)> AddBook(FileData file);
        Task<List<BookInfo>> LoadBooks();
        Task<BookInfo> LoadBookById(string id);
        void RemoveById(string id);
        void SaveBook(BookInfo info);
        void Clear();
        Task<BookInfo> LoadMostRecentBook();
    }
}
