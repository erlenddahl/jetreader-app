using System;
using System.Collections.Generic;
using System.Text;

namespace EbookReader.BookLoaders
{
    public class EbookChapter
    {
        public string Title { get; }
        public string Content { get; }
        public string Href { get; }
        public int Depth { get; }

        public EbookChapter(string title, string content, string href = "", int depth = 0)
        {
            Href = href;
            Title = title;
            Content = content;
            Depth = depth;
        }
    }
}
