﻿using EbookReader.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Model.Messages
{
    public class ChangedBookmarkNameMessage {
        public Bookmark Bookmark { get; set; }
    }
}
