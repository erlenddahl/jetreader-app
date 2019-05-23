using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using EbookReader;
using EbookReader.BookLoaders.Epub;
using EbookReader.Service;
using EpubSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JetReader.Tests
{
    [TestClass]
    public class EpubLoaderTests
    {
        //public static string LibraryPath = @"C:\Users\erlendd\Dropbox\Bøker\Calibre-bibliotek";
        public static string LibraryPath = @"C:\Users\Erlend\Dropbox\Bøker\Calibre-bibliotek";

        public static string[] GetEpubs()
        {
            return Directory.GetFiles(LibraryPath, "*.epub", SearchOption.AllDirectories);
        }

        public static string GetMd5(byte[] bytes)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hashBytes = md5.ComputeHash(bytes);

                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        public static byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        [TestMethod]
        public void TestAllEpubs()
        {
            //Successes: 310 / 408
            //Time usage: 112,7570463 seconds

            var epubs = GetEpubs();
            var failures = new List<string>();

            IocManager.Build();

            var fs = new FileService();
            var loader = new EpubLoader(fs);
            var start = DateTime.Now;

            foreach (var file in epubs)
            {
                try
                {
                    loader.OpenBook(file).Wait();
                }
                catch (Exception ex)
                {
                    failures.Add(Path.GetDirectoryName(file) + ": " + ex.Message + (string.Join("; ", (ex as AggregateException)?.InnerExceptions.Select(p => p.Message))));
                }
            }

            Console.WriteLine("Successes: " + (epubs.Length - failures.Count) + " / " + epubs.Length);
            Console.WriteLine("Time usage: " + DateTime.Now.Subtract(start).TotalSeconds + " seconds");

            foreach (var failure in failures)
                Console.WriteLine(failure);
        }

        [TestMethod]
        public void TestAllEpubsEpubsharp()
        {
            //Successes: 310 / 408
            //Time usage: 112,7570463 seconds

            var epubs = GetEpubs();
            var failures = new List<string>();
            var start = DateTime.Now;

            foreach (var file in epubs)
            {
                byte[] data;
                using (var r = File.OpenRead(file))
                {
                    data = ReadFully(r);
                }
                var id = GetMd5(data);

                try
                {
                    var book = EpubReader.Read(file);
                    Debug.WriteLine(book.Title);
                }
                catch (Exception ex)
                {
                    failures.Add(Path.GetDirectoryName(file) + ": " + ex.Message + (string.Join("; ", ((ex as AggregateException)?.InnerExceptions?.Select(p => p.Message)) ?? new string[0])));
                }
            }

            Console.WriteLine("Successes: " + (epubs.Length - failures.Count) + " / " + epubs.Length);
            Console.WriteLine("Time usage: " + DateTime.Now.Subtract(start).TotalSeconds + " seconds");

            foreach (var failure in failures)
                Console.WriteLine(failure);
        }

        [TestMethod]
        public void TestAllEpubsVersOne()
        {
            //Successes: 310 / 408
            //Time usage: 112,7570463 seconds

            var epubs = GetEpubs();
            var failures = new List<string>();
            var start = DateTime.Now;

            foreach (var file in epubs)
            {
                byte[] data;
                using (var r = File.OpenRead(file))
                {
                    data = ReadFully(r);
                }
                var id = GetMd5(data);

                try
                {
                    var book = VersOne.Epub.EpubReader.ReadBook(file);
                    Debug.WriteLine(book.Schema.Package);
                }
                catch (Exception ex)
                {
                    failures.Add(Path.GetDirectoryName(file) + ": " + ex.Message + (string.Join("; ", ((ex as AggregateException)?.InnerExceptions?.Select(p => p.Message)) ?? new string[0])));
                }
            }

            Console.WriteLine("Successes: " + (epubs.Length - failures.Count) + " / " + epubs.Length);
            Console.WriteLine("Time usage: " + DateTime.Now.Subtract(start).TotalSeconds + " seconds");

            foreach (var failure in failures)
                Console.WriteLine(failure);
        }
    }
}
