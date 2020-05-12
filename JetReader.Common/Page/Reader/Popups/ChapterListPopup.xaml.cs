using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using JetReader.BookLoaders;
using JetReader.Books;
using JetReader.Model.Format;
using JetReader.Service;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Reader.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChapterListPopup : PopupPage
    {
        private List<EbookChapter> _chapters;
        private int _selectedTab;
        private ObservableCollection<Bookmark> _bookmarks;
        private BookInfo _bookInfo;
        private readonly IBookmarkService _bookmarkService;

        public event EventHandler<EbookChapter> OnChapterClicked;
        public event EventHandler<Bookmark> OnBookmarkClicked;

        public List<EbookChapter> Chapters
        {
            get => _chapters;
            private set
            {
                _chapters = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Bookmark> Bookmarks
        {
            get => _bookmarks;
            private set
            {
                _bookmarks = value;
                OnPropertyChanged();
            }
        }

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value; 
                OnPropertyChanged(nameof(ChaptersVisible));
                OnPropertyChanged(nameof(BookmarksVisible));
            }
        }

        public bool ChaptersVisible => SelectedTab == 0;
        public bool BookmarksVisible => SelectedTab == 1;

        public ChapterListPopup()
        {
            _bookmarkService = IocManager.Container.Resolve<IBookmarkService>();
            InitializeComponent();
        }

        private void ChapterTapped(object sender, ItemTappedEventArgs e)
        {
            OnChapterClicked?.Invoke(this, e.Item as EbookChapter);
        }

        private async void BookmarkTapped(object sender, ItemTappedEventArgs e)
        {
            var result = await DisplayActionSheet("Bookmark", "Cancel", null, "Go to", "Edit", "Remove");

            var bm = e.Item as Bookmark;
            if (result == "Go to")
                OnBookmarkClicked?.Invoke(this, bm);
            else if (result == "Edit")
            {
                
            }
            else if (result == "Remove")
            {
                _bookmarkService.DeleteBookmark(bm, _bookInfo.Id);
                Bookmarks.Remove(bm);
            }
        }

        private void SelectContentTab(object sender, EventArgs e)
        {
            SelectedTab = 0;
        }

        private void SelectBookmarksTab(object sender, EventArgs e)
        {
            SelectedTab = 1;
        }

        private void AddBookmarkClicked(object sender, EventArgs e)
        {
            Bookmarks.Add(_bookmarkService.CreateBookmark(DateTime.Now.ToString(), _bookInfo.Id, _bookInfo.Position));
        }

        public async void SetBook(Ebook ebook, BookInfo bookInfo)
        {
            _bookInfo = bookInfo;
            Chapters = ebook.TableOfContents;
            await RefreshBookmarks();
        }

        private void DeleteBookmark(object sender, EventArgs e)
        {
            Debug.WriteLine(sender);
        }

        private void EditBookmark(object sender, EventArgs e)
        {
            Debug.WriteLine(sender);
        }

        public async Task RefreshBookmarks()
        {
            Bookmarks = new ObservableCollection<Bookmark>(await _bookmarkService.LoadBookmarksByBookId(_bookInfo.Id));
        }
    }
}