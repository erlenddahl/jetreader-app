using System;
using System.Collections.Generic;
using System.Linq;
using EbookReader.Model.WebViewMessages;

namespace EbookReader.Books
{
    public class ReadingStatistics
    {
        public List<ReadingDate> Dates { get; set; } = new List<ReadingDate>();

        public void OpenedBook()
        {
            var date = GetCurrent();
            date.SessionCount++;
        }

        public ReadingDate GetCurrent()
        {
            return Dates.FirstOrDefault(p => p.Date == DateTime.Now.Date) ?? new ReadingDate();
        }

        public void Save(ReadStats readStats)
        {
            var date = GetCurrent();
            date.ToProgress = readStats.Progress;
            if (date.FromProgress < 0) date.FromProgress = readStats.Progress;
            date.PageTurns++;
            date.Seconds += readStats.Seconds;
            date.Words += readStats.Words;
        }
    }
}