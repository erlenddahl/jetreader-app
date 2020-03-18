using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.Books;
using JetReader.Model.Messages;
using JetReader.Model.View;
using JetReader.Service;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Reader.QuickPanelTab
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Bookmarks : StackLayout {
        public Bookmarks() {
            InitializeComponent();

            BindingContext = new BookmarksVm();

            IocManager.Container.Resolve<IMessageBus>().Subscribe<BookmarksChangedMessage>(BookmarksChangedSubsciber);
        }

        public void SetBookmarks(List<Bookmark> items) {
            Device.BeginInvokeOnMainThread(() => {
                SetItems(items);
            });
        }

        private void SetItems(List<Bookmark> items) {

            Items.Children.Clear();

            foreach (var item in GetItems(items)) {
                Items.Children.Add(item);
            }
        }

        private List<StackLayout> GetItems(List<Bookmark> items) {

            var layouts = new List<StackLayout>();

            foreach (var item in items.Where(o => !o.Deleted).OrderBy(o => o.Position.Spine).ThenBy(o => o.Position.SpinePosition)) {
                layouts.Add(new BookmarksTab.Bookmark(item));
            }

            return layouts;
        }

        private void BookmarksChangedSubsciber(BookmarksChangedMessage msg) {
            SetBookmarks(msg.Bookmarks);
        }
    }
}