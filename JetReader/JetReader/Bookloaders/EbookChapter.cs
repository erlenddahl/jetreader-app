using JetReader.Helpers;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace JetReader.BookLoaders
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

        public (int wordCount, int letterCount) GetStatistics()
        {
            var rawHtml = Content;

            var doc = new HtmlDocument();
            doc.LoadHtml(rawHtml);

            HtmlHelper.StripHtmlTags(doc, "script", "style", "img", "svg", "br");

            var text = HtmlHelper.GetBody(doc).InnerText;

            if (text.Contains("<"))
                Debug.WriteLine("Text still contains HTML tags!");

            text = new string(text.ToCharArray().Select(p => char.IsLetterOrDigit(p) ? p : ' ').ToArray()).Trim();

            while (text.Contains("  ")) text = text.Replace("  ", " ");

            return (CountSpaces(text) + 1, text.Length);
        }

        /// <summary>
        /// Counts the number of spaces in the given string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private int CountSpaces(string text)
        {
            // Using a manual for loop because testing showed it was quicker than both string.Split and linq.Count
            // (and uses less memory than the former).
            var count = 0;
            for (var j = 0; j < text.Length; j++)
                if (text[j] == ' ') count++;
            return count;
        }

        /// <summary>
        /// Returns the Ebook-absolute path of the given path relative to the path of this chapter.
        /// Examples:
        ///     chapter.Href = 'Text/text0001.html', path = 'text0002.html' => 'Text/text0002.html'
        ///     chapter.Href = 'Text/text0001.html', path = '../Img/img0001.png' => 'Img/img0001.html'
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ResolveRelativePath(string path)
        {
            return PathHelper.CombinePath(Href, path);
        }
    }
}
