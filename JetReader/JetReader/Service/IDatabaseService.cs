﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace JetReader.Service {
    public interface IDatabaseService {
        SQLiteAsyncConnection Connection { get; }
    }
}
