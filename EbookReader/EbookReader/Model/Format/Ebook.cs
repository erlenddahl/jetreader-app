using EbookReader.BookLoaders;
using EpubSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Model.Bookshelf;

namespace EbookReader.Model.Format
{

    public class EpubEbook : Ebook
    {
        public EpubBook Data { get; }

        public EpubEbook(string path, EpubBook book)
        {
            Path = path;
            Title = book.Title;
            Author = string.Join(", ", book.Authors);
            Description = book.Format.Opf.Metadata.Descriptions.FirstOrDefault() ?? "";
            Language = book.Format.Opf.Metadata.Languages.FirstOrDefault() ?? "";
            Chapters = GetChapters(book).ToList();
            Cover = book.CoverImage;
            Data = book;
        }

        private IEnumerable<EbookChapter> GetChapters(EpubBook book)
        {
            var dict = book.Resources.Html.ToDictionary(k => k.FileName, v => v);
            foreach (var chapter in book.TableOfContents)
            {
                var c = dict[chapter.FileName];
                yield return new EbookChapter(chapter.Title, c.TextContent, c.FileName);
                if (chapter.SubChapters.Any())
                    foreach (var sc in RecurseSubchapters(1, dict, chapter.SubChapters))
                        yield return sc;
            }
        }
        private IEnumerable<EbookChapter> RecurseSubchapters(int depth, Dictionary<string, EpubTextFile> dict, IList<EpubChapter> subChapters)
        {
            foreach (var chapter in subChapters)
            {
                var c = dict[chapter.FileName];
                yield return new EbookChapter(chapter.Title, c.TextContent, c.FileName, depth);
                if (chapter.SubChapters.Any())
                    foreach (var sc in RecurseSubchapters(depth + 1, dict, chapter.SubChapters))
                        yield return sc;
            }
        }
    }

    public class Ebook {

        public string Path { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public List<EbookChapter> Chapters { get; set; }
        public byte[] Cover { get; set; }

        public EbookFormat Format { get; set; }

        public virtual Book ToBookshelf()
        {
            return new Book
            {
                Title = Title,
                Path = Path,
                Cover = Cover,
                Format = Format,
            };
        }
    }
}
