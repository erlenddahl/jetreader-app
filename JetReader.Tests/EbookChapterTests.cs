using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetReader;
using JetReader.BookLoaders;
using JetReader.BookLoaders.Epub;
using JetReader.Model.Format;
using JetReader.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JetReader.Tests
{
	[TestClass]
	public class EbookChapterTests
	{
		[TestMethod]
		public void ResolveRelativePathTests()
        {
            var chapter = new EbookChapter("", "", "Text/text0001.html");

            Assert.AreEqual("Text/text0002.html", chapter.ResolveRelativePath("text0002.html"));
            Assert.AreEqual("text0002.html", chapter.ResolveRelativePath("../text0002.html"));
            Assert.AreEqual("Text/text0002.html", chapter.ResolveRelativePath("../Text/text0002.html"));
            Assert.AreEqual("Imgs/cover.png", chapter.ResolveRelativePath("../Imgs/cover.png"));
            Assert.AreEqual("cover.png", chapter.ResolveRelativePath("../cover.png"));

            chapter = new EbookChapter("", "", "text0001.html");

            Assert.AreEqual("text0002.html", chapter.ResolveRelativePath("text0002.html"));
            Assert.AreEqual("text0002.html", chapter.ResolveRelativePath("../text0002.html"));
            Assert.AreEqual("Text/text0002.html", chapter.ResolveRelativePath("../Text/text0002.html"));
            Assert.AreEqual("Imgs/cover.png", chapter.ResolveRelativePath("../Imgs/cover.png"));
            Assert.AreEqual("cover.png", chapter.ResolveRelativePath("../cover.png"));
            Assert.AreEqual("cover.png", chapter.ResolveRelativePath("/cover.png"));
        }

        [TestMethod]
        public void StatisticsTests()
        {
            var epubs = EpubLoaderTests.GetEpubs();

            IocManager.Build();

            var fs = new FileService();
            var loader = new EpubLoader(fs);

            foreach (var file in epubs)
            {
                Ebook book = null;
                try
                {
                    book = loader.OpenBook(file).Result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(System.IO.Path.GetFileNameWithoutExtension(file) + ": " + ex.Message);
                    continue;
                }

                var words = 0;
                var letters = 0;
                foreach (var p in book.HtmlFiles)
                {
                    var (wordsC, lettersC) = p.GetStatistics();
                    words += wordsC;
                    letters += lettersC;
                    if (letters < 1) continue;
                    Assert.IsTrue(words > 0);
                    Assert.IsTrue(letters > 0);
                    Assert.IsTrue(letters > words);
                }

                Console.WriteLine($"{book.Author} - {book.Title}: {words:n0} words, {letters:n0} letters.");
            }
        }
    }
}
