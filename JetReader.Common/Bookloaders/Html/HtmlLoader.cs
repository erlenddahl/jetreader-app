using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Model.Format;
using JetReader.Service;
using HtmlAgilityPack;

namespace JetReader.BookLoaders.Html
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
