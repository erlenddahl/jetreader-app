using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetReader.Books;
using JetReader.DependencyService;
using SQLite;

namespace JetReader.Service
{
    public class DatabaseService : IDatabaseService {
        readonly SQLiteAsyncConnection _database;

        public SQLiteAsyncConnection Connection => _database;

        public DatabaseService(IFileHelper fileHelper) {
            var dbPath = fileHelper.GetLocalFilePath(AppSettings.Bookshelft.SqlLiteFilename);
            _database = new SQLiteAsyncConnection(dbPath);
            CreateTables();
        }

        private void CreateTables() {
            try
            {
                _database.CreateTableAsync<BookInfo>().Wait();
                _database.CreateTableAsync<Bookmark>().Wait();
            }catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
