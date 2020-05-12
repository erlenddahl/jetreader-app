using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.BookLoaders;
using JetReader.Model.Format;
using JetReader.Service;
using Newtonsoft.Json;
using SQLite;

namespace JetReader.Books
{
    public class BookInfo {
        [PrimaryKey]
        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime? BookmarksSyncLastChange { get; set; }
        public EbookFormat Format { get; set; }
        public DateTime? FinishedReading { get; set; }
        public DateTime LastRead { get; set; }
        public string BookLocation { get; set; }
        public string CoverFilename { get; set; }

        [Ignore]
        public ReadingStatistics ReadStats
        {
            get => _readStats ?? (_readStats = new ReadingStatistics());
            set => _readStats = value;
        }

        public string ReadingStatisticsJson
        {
            get => JsonConvert.SerializeObject(ReadStats);
            set => ReadStats = string.IsNullOrWhiteSpace(value) ? new ReadingStatistics() : JsonConvert.DeserializeObject<ReadingStatistics>(value);
        }

        [Ignore]
        public List<ChapterData> ChapterInfo { get; set; }
        public string ChapterDataJson {
            get => JsonConvert.SerializeObject(ChapterInfo);
            set => ChapterInfo = string.IsNullOrWhiteSpace(value) ? new List<ChapterData>() : JsonConvert.DeserializeObject<List<ChapterData>>(value);
        }

        public int Spine { get; set; }
        public int SpinePosition { get; set; }

        [Ignore]
        public virtual Position Position {
            get => new Position(Spine, SpinePosition);
            set {
                Spine = value.Spine;
                SpinePosition = value.SpinePosition;
            }
        }

        public long BookFileSize { get; set; }

        private int _processingStatus = 0;
        private ReadingStatistics _readStats;

        private string GetTempLocation()
        {
            return FileService.ToAbsolute(Id);
        }

        /// <summary>
        /// Does the required heavy processing async. This should only be done once per book (when adding it to the book shelf). The variable
        /// _processingStatus will be set to 1 when processing is started, and 2 when it is finished. If it is not 0, the function will return immediately.
        /// When heavy processing data is needed, you should await WaitForProcessingToFinish() to make sure it has been processed.
        /// </summary>
        /// <param name="ebook"></param>
        /// <returns></returns>
        public async Task ProcessBook(Ebook ebook)
        {
            if (_processingStatus != 0) return;
            _processingStatus = 1;

            if (ChapterInfo?.Any() ?? false)
            {
                // Already loaded (from book shelf, probably)
                _processingStatus = 2;
                return;
            }

            // Run the processing in parallel
            var fs = IocManager.Container.Resolve<FileService>();
            await Task.Run(() => { BookFileSize = fs.GetFileSizeInBytes(BookLocation).Result; });
            await Task.Run(() => { ChapterInfo = ebook.HtmlFiles.Select(p => new ChapterData(p)).ToList(); });

            _processingStatus = 2;

            // Save the processed information
            var shelf = IocManager.Container.Resolve<BookshelfService>();
            shelf.SaveBook(this);
        }

        /// <summary>
        /// Waits in increments of 25ms until the heavy processing has been finished.
        /// </summary>
        /// <returns></returns>
        public async Task WaitForProcessingToFinish()
        {
            while (_processingStatus < 2)
                await Task.Delay(25);
        }

        public async Task DeleteTempLocation(FileService fs)
        {
            await fs.DeleteFolder(GetTempLocation());
        }

        public async Task ExtractToTemp(FileService fs, Ebook book)
        {
            await fs.CreateDirectoryAsync(GetTempLocation());

            foreach (var (filename, filedata) in book.ExtractFiles)
            {
                var path = GetTempPath(filename);
                await fs.WriteBytesAsync(path, filedata);
            }
        }

        public string GetTempPath(string filename)
        {
            return System.IO.Path.Combine(GetTempLocation(), filename.Replace("/", "_-_").Replace("\\", "_-_"));
        }

        public void PrintTempFiles()
        {
            foreach (var file in System.IO.Directory.GetFiles(GetTempLocation(), "*.*"))
                Debug.WriteLine(file);
        }

        /// <summary>
        /// Counts the number of words in every chapter before the current chapter, in the current chapter,
        /// and after the current chapter. Used for estimating the reading progress.
        /// </summary>
        /// <param name="chapter"></param>
        /// <returns></returns>
        public (int wordsBefore, int wordsCurrent, int wordsAfter) GetWordCountsAround(EbookChapter chapter)
        {
            var (before, current, after) = (0, 0, 0);

            var isBefore = true;
            foreach (var c in ChapterInfo)
            {
                if (c.Href == chapter.Href)
                {
                    isBefore = false;
                    current = c.Words;
                }
                else
                {
                    if (isBefore)
                        before += c.Words;
                    else
                        after += c.Words;
                }
            }

            return (before, current, after);
        }

        public bool HasStats()
        {
            return ReadStats?.Dates?.Any(p => p.Seconds > 1) == true;
        }
    }
}