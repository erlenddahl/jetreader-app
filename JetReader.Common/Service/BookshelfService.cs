using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.Books;
using JetReader.DependencyService;
using JetReader.Helpers;
using JetReader.Repository;
using Plugin.FilePicker.Abstractions;

namespace JetReader.Service {
    public class BookshelfService : IBookshelfService {
        readonly FileService _fileService;
        readonly IBookRepository _bookRepository;
        readonly IBookmarkRepository _bookmarkRepository;

        public BookshelfService(FileService fileService, IBookRepository bookRepository, IBookmarkRepository bookmarkRepository) {
            _fileService = fileService;
            _bookRepository = bookRepository;
            _bookmarkRepository = bookmarkRepository;
        }

        public async Task<(BookInfo, bool)> AddBook(FileData file) {

            var newBook = false;
            var bookLoader = EbookFormatHelper.GetBookLoader(file.FileName);

            var id = await _fileService.GetFileHash(file.FilePath);
            var bsBook = await _bookRepository.GetBookByIdAsync(id);

            if (bsBook == null) {
                var ebook = await bookLoader.OpenBook(file.FilePath);
                bsBook = ebook.Info;
                await bsBook.ExtractToTemp(_fileService, ebook);
                await _bookRepository.SaveBookAsync(bsBook);
                newBook = true;
            }

            return (bsBook, newBook);
        }

        public async Task<List<BookInfo>> LoadBooks() {
            return await _bookRepository.GetAllBooksAsync();
        }
        
        public async void RemoveById(string id) {
            var book = await _bookRepository.GetBookByIdAsync(id);
            if (book == null) return;

            await book.DeleteTempLocation(_fileService);
            var bookmarks = await _bookmarkRepository.GetBookmarksByBookIdAsync(id);
            foreach(var bookmark in bookmarks) {
                await _bookmarkRepository.DeleteBookmarkAsync(bookmark);
            }
            await _bookRepository.DeleteBookAsync(book);
        }

        public async void SaveBook(BookInfo book) {
            await _bookRepository.SaveBookAsync(book);
        }

        public void Clear()
        {
            _bookRepository.DeleteAllBooksAsync();
        }

        public Task<BookInfo> LoadMostRecentBook()
        {
            return _bookRepository.GetMostRecentBook();
        }

        public async Task<BookInfo> LoadBookById(string id) {
            return await _bookRepository.GetBookByIdAsync(id);
        }
    }
}