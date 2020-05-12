using System.Collections.Generic;
using System.Linq;
using JetReader.BookLoaders;
using JetReader.Books;
using EpubSharp;
using HtmlAgilityPack;
using JetReader.Extensions;

namespace JetReader.Model.Format
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
            TableOfContents = FlattenToc(book.TableOfContents).ToList();
            Data = book;

            if (book.CoverImage != null && book.CoverImage.Length > 0)
            {
                CoverFilename = book.Resources.Images.FirstOrDefault(p => p.Content == book.CoverImage)?.FileName;
                CoverData = book.CoverImage;
            }

            GenerateInfo();
        }

        private IEnumerable<EbookChapter> FlattenToc(IEnumerable<EpubChapter> toc, int depth = 0)
        {
            foreach (var chapter in toc)
            {
                yield return ToEbookChapter(chapter, depth);
                if(chapter.SubChapters.Any())
                    foreach (var subchapter in FlattenToc(chapter.SubChapters, depth + 1))
                        yield return subchapter;
            }
        }

        private EbookChapter ToEbookChapter(EpubChapter c, int depth)
        {
            return new EbookChapter(c.Title, "", c.FileName + "#" + c.Anchor, depth);
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
            var dict = book.TableOfContents.FlattenWithDepth(p => p.SubChapters).ToDictionary(k => k.Item.FileName, v => v);
            var titleDict = dict.ToDictionary(k => k.Key, v => v.Value.Item);
            foreach (var chapter in book.SpecialResources.HtmlInReadingOrder)
            {
                yield return new EbookChapter(GetTitle(titleDict, chapter), chapter.TextContent, chapter.FileName, dict.TryGetValue(chapter.FileName, out var c) ? c.Depth : 0);
            }
        }
    }
}