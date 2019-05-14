﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.DependencyService;
using EbookReader.Model.Bookshelf;
using SQLite;

namespace EbookReader.Service {
    public class DatabaseService : IDatabaseService {
        readonly SQLiteAsyncConnection _database;

        public SQLiteAsyncConnection Connection => _database;

        public DatabaseService(IFileHelper fileHelper) {
            var dbPath = fileHelper.GetLocalFilePath(AppSettings.Bookshelft.SqlLiteFilename);
            _database = new SQLiteAsyncConnection(dbPath);
            CreateTables();
        }

        private void CreateTables() {
           _database.CreateTableAsync<Book>().Wait();
           _database.CreateTableAsync<Bookmark>().Wait();
        }
    }
}
