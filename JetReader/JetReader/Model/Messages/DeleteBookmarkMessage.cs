using JetReader.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Model.Messages
{
    public class DeleteBookmarkMessage {
        public Bookmark Bookmark { get; set; }
    }
}
