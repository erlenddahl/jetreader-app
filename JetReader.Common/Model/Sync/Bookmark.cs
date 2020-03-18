using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Books;
using Newtonsoft.Json;

namespace JetReader.Model.Sync
{
    public class Bookmark {

        [JsonProperty("I")]
        public long Id { get; set; }

        [JsonProperty("D")]
        public bool Deleted { get; set; }

        [JsonProperty("S")]
        public int Spine { get; set; }

        [JsonProperty("P")]
        public int Position { get; set; }

        [JsonProperty("N")]
        public string Name { get; set; }

        [JsonProperty("C")]
        public DateTime LastChange { get; set; }

        public static Bookmark FromDbBookmark(Books.Bookmark bm)
        {
            return new Bookmark()
            {
                Id = bm.Id,
                Name = bm.Name,
                Position = bm.Position.SpinePosition,
                Spine = bm.Position.Spine,
                Deleted = bm.Deleted,
                LastChange = bm.LastChange,
            };
        }

        public Books.Bookmark ToDbBookmark(string bookId)
        {
            return new Books.Bookmark()
            {
                Id = Id,
                BookId = bookId,
                Name = Name,
                Position = new Position(Spine, Position),
                LastChange = DateTime.UtcNow,
            };
        }
    }
}
