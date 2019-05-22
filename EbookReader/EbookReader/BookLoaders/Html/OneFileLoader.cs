using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public virtual async Task<Ebook> OpenBook(string filePath)
        {
            var title = Path.GetFileName(filePath);
            var data = await _fileService.ReadAllTextAsync(filePath);

            var book = new Ebook()
            {
                Title = title,
                Chapters = new List<EbookChapter>() { new EbookChapter(title, data) },
            };

            return book;
        }

        public virtual async Task<string> PrepareHtml(string html, Ebook book, EbookChapter chapter)
        {
            return await Task.Run(() =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                StripHtmlTags(doc);

                return doc.DocumentNode.Descendants("body").First().InnerHtml;
            });
        }

        protected virtual void StripHtmlTags(HtmlDocument doc)
        {
            var tagsToRemove = new[] { "script", "style", "iframe" };
            var nodesToRemove = doc.DocumentNode
                .Descendants()
                .Where(o => tagsToRemove.Contains(o.Name))
                .ToList();

            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }
    }
}
