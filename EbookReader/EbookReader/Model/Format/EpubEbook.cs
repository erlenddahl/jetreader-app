using System.Collections.Generic;
using System.Linq;
using EbookReader.BookLoaders;
using EbookReader.Books;
using EpubSharp;
using HtmlAgilityPack;

namespace EbookReader.Model.Format
{
    public class EpubEbook : Ebook
    {
        public EpubBook Data { get; }

        public override IEnumerable<(string filename, byte[] filedata)> ExtractFiles
        {
            get
            {
                return
                    Data.Resources.Fonts
                        .Union(Data.Resources.Images)
                        .Union(Data.Resources.Other.Where(p => p.MimeType.ToLower().Contains("font")))
                        .Select(p => (p.FileName, p.Content));
            }
        }

        public EpubEbook(EpubBook book, string path, BookInfo info = null) : base(path, info)
        {
            Title = book.Title;
            Author = string.Join(", ", book.Authors);
            Description = book.Format.Opf.Metadata.Descriptions.FirstOrDefault() ?? "";
            Language = book.Format.Opf.Metadata.Languages.FirstOrDefault() ?? "";
            HtmlFiles = GetHtmlFilesInReadingOrder(book).ToList();
            Data = book;

            if (book.CoverImage != null && book.CoverImage.Length > 0)
            {
                CoverFilename = book.Resources.Images.FirstOrDefault(p => p.Content == book.CoverImage)?.FileName;
                CoverData = book.CoverImage;
            }

            GenerateInfo();
        }

        private string GetTitle(Dictionary<string, EpubChapter> dict, EpubTextFile text)
        {
            // First, see if the file is in the TOC.
            if (dict.TryGetValue(text.FileName, out var nav))
                if (!string.IsNullOrWhiteSpace(nav.Title))
                    return nav.Title;

            // If not, try to extract the title from the HTML code.
            var html = new HtmlDocument();
            html.LoadHtml(text.TextContent);
            var title = html.DocumentNode.SelectSingleNode("//head/title")?.InnerText ?? html.DocumentNode.SelectSingleNode("//title")?.InnerText;
            if (!string.IsNullOrWhiteSpace(title)) return title;

            // If nothing else works, return the file name.
            return System.IO.Path.GetFileNameWithoutExtension(text.FileName);
        }

        private IEnumerable<EbookChapter> GetHtmlFilesInReadingOrder(EpubBook book)
        {
            var dict = book.TableOfContents.ToDictionary(k => k.FileName, v => v);
            foreach (var chapter in book.SpecialResources.HtmlInReadingOrder)
            {
                yield return new EbookChapter(GetTitle(dict, chapter), chapter.TextContent, chapter.FileName);
            }
        }
    }
}