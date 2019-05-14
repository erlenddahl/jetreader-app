using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Model.Format;

namespace EbookReader.Service {
    public interface IBookLoader {
        Task<Ebook> GetBook(string filename, byte[] fileData, string bookId);
        Task<Ebook> OpenBook(string path);
        Task<string> GetChapter(Ebook book, Spine chapter);
        Task<Model.EpubLoader.HtmlResult> PrepareHtml(string html, Ebook book, File chapter);
        Model.Bookshelf.Book CreateBookshelfBook(Ebook book);
    }
}
