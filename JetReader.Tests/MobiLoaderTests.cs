using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JetReader.BookLoaders.Epub;
using JetReader.Model.Format.Mobi;
using JetReader.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JetReader.Tests
{
    [TestClass]
    public class MobiLoaderTests
    {
        public static string LibraryPath = @"F:\Dropbox\Bøker\Calibre-bibliotek";

        public static string[] GetMobis()
        {
            return Directory.GetFiles(LibraryPath, "*.mobi", SearchOption.AllDirectories);
        }

        [TestMethod]
        public void TestSingleMobi()
        {
            var book = new MobiFile(new FileStream(GetMobis().First(), FileMode.Open));

            Assert.AreEqual(629855, book.BookText.Length);
            Assert.AreEqual(@"<html><head><guide><reference type=""toc"" title=""Table of Contents"" filepos=0000647004 /></guide></he", book.BookText.Substring(0, 100));
            Assert.AreEqual(" ></a></body></html>", book.BookText.Substring(book.BookText.Length - 20, 20));
        }

        [TestMethod]
        public void TestAllMobis()
        {
            //Successes: 300 / 328
            //Time usage: 17,36 seconds

            var epubs = GetMobis();
            var failures = new List<string>();
            var stacktrace = "";

            var start = DateTime.Now;

            foreach (var file in epubs)
            {
                try
                {
                    var book = new MobiFile(new FileStream(file, FileMode.Open));
                }
                catch (Exception ex)
                {
                    failures.Add(Path.GetDirectoryName(file) + ": " + ex.Message);
                    stacktrace = ex.StackTrace;
                }
            }

            Console.WriteLine("Successes: " + (epubs.Length - failures.Count) + " / " + epubs.Length);
            Console.WriteLine("Time usage: " + DateTime.Now.Subtract(start).TotalSeconds + " seconds");

            foreach (var failure in failures)
                Console.WriteLine(failure);

            Console.WriteLine(stacktrace);
        }
    }
}
