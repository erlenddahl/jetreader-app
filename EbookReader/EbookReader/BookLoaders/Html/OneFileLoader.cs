using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Helpers;
using EbookReader.Model.Bookshelf;
using EbookReader.Model.Format;
using EbookReader.Service;
using EpubSharp;
using HtmlAgilityPack;

namespace EbookReader.BookLoaders.Html
{
    public abstract class OneFileLoader : IBookLoader
    {

        protected string[] Extensions;

        protected EbookFormat EbookFormat;
        private FileService _fileService;

        public OneFileLoader(FileService fileService)
        {
            _fileService = fileService;
        }

        public virtual async Task<Ebook> OpenBook(string filePath, string id = null)
        {
            var title = Path.GetFileName(filePath);
            var data = await _fileService.ReadAllTextAsync(filePath);

            var book = new Ebook()
            {
                Id = id ?? await _fileService.GetFileHash(filePath),
                Title = title,
                HtmlFiles = new List<EbookChapter>() { new EbookChapter(title, data) },
            };

            return book;
        }

        public virtual async Task<string> PrepareHtml(string html, Ebook book, EbookChapter chapter)
        {
            return await Task.Run(() =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlHelper.StripHtmlTags(doc, new[] { "script", "style", "iframe" });

                return HtmlHelper.GetBody(doc).InnerHtml;
            });
        }
    }
}
