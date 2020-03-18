using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.BookLoaders.Html;
using JetReader.Model.Format;
using JetReader.Service;

namespace JetReader.BookLoaders.Txt
{
    public class TxtLoader : OneFileLoader
    {

        public TxtLoader(FileService fileService) : base(fileService)
        {
            Extensions = new[] { "txt" };
            EbookFormat = EbookFormat.Txt;
        }

        public override async Task<string> PrepareHtml(string html, Ebook book, EbookChapter chapter)
        {
            html = $"<body><p>{html}</p></body>".Replace("\n", "</p><p>");
            return await base.PrepareHtml(html, book, chapter);
        }
    }
}