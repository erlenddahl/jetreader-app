using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using EbookReader.DependencyService;
using EbookReader.Helpers;
using EbookReader.Model.Bookshelf;
using EbookReader.Repository;
using Plugin.FilePicker.Abstractions;

namespace EbookReader.Service {
    public class BookshelfService : IBookshelfService {
        readonly FileService _fileService;
        readonly ICryptoService _cryptoService;
        readonly IBookRepository _bookRepository;
        readonly IBookmarkRepository _bookmarkRepository;

        public BookshelfService(FileService fileService, ICryptoService cryptoService, IBookRepository bookRepository, IBookmarkRepository bookmarkRepository) {
            _fileService = fileService;
            _cryptoService = cryptoService;
            _bookRepository = bookRepository;
            _bookmarkRepository = bookmarkRepository;
        }

        public async Task<(Book, bool)> AddBook(FileData file) {

            var newBook = false;
            var bookLoader = EbookFormatHelper.GetBookLoader(file.FileName);

            var id = _cryptoService.GetMd5(file.DataArray);

            var bsBook = await _bookRepository.GetBookByIdAsync(id);

            if (bsBook == null) {
                var ebook = await bookLoader.OpenBook(file.FilePath);
                bsBook = ebook.ToBookshelf(id);
                await bsBook.CreateTempLocation(_fileService);
                await bsBook.SaveToTempLocation(_fileService, ebook.CoverFilename, ebook.CoverData);
                await _bookRepository.SaveBookAsync(bsBook);
                newBook = true;
            }

            return (bsBook, newBook);
        }

        public async Task<List<Book>> LoadBooks() {
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

        public async void SaveBook(Book book) {
            await _bookRepository.SaveBookAsync(book);
        }

        public void Clear()
        {
            _bookRepository.DeleteAllBooksAsync();
        }

        public async Task<Book> LoadBookById(string id) {
            return await _bookRepository.GetBookByIdAsync(id);
        }
    }
}