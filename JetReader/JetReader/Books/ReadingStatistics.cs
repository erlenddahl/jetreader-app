using System;
using System.Collections.Generic;
using System.Linq;
using JetReader.Model.WebViewMessages;

namespace JetReader.Books
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
            var d = Dates.FirstOrDefault(p => p.Date == DateTime.Now.Date);
            if (d == null)
            {
                d = new ReadingDate();
                Dates.Add(d);
            }

            return d;
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