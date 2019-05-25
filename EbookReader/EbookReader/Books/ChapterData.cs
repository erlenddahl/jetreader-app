using EbookReader.BookLoaders;

namespace EbookReader.Books
{
    public class ChapterData
    {
        public string Title { get; set; }
        public int Words { get; set; }
        public int Letters { get; set; }

        public string Href { get; set; }

        public ChapterData()
        {

        }

        public ChapterData(EbookChapter chapter)
        {
            Title = chapter.Title;
            Href = chapter.Href;
            (Words, Letters) = chapter.GetStatistics();
        }
    }
}