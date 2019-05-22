using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Model.EpubLoader;
using EbookReader.Model.Format;
using EbookReader.Service;
using HtmlAgilityPack;

namespace EbookReader.BookLoaders.Html
{
    public class HtmlLoader : OneFileLoader
    {
        public HtmlLoader(FileService fileService) : base(fileService)
        {
            Extensions = new[] { "html", "xhtml", "htm", "xhtm" };
            EbookFormat = EbookFormat.Html;
        }
    }
}
