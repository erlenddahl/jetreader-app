using System;

namespace EbookReader.Books
{
    public class ReadingDate
    {
        /// <summary>
        /// The date these statistics are from.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The device these statistics are from.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// The number of words read on this date.
        /// </summary>
        public double Words { get; set; }

        /// <summary>
        /// The total number of seconds spent reading on this date.
        /// </summary>
        public double Seconds { get; set; }

        /// <summary>
        /// The reading progress when reading started on this date.
        /// </summary>
        public double FromProgress { get; set; }

        /// <summary>
        /// The reading progress when reading finished on this date.
        /// </summary>
        public double ToProgress { get; set; }

        /// <summary>
        /// The number of page turns on this date.
        /// </summary>
        public int PageTurns { get; set; }

        /// <summary>
        /// The number of reading sessions on this date.
        /// </summary>
        public int SessionCount { get; set; }

        public ReadingDate()
        {
            Date = DateTime.Now.Date;
            FromProgress = -1;
            DeviceId = UserSettings.DeviceId;
        }
    }
}