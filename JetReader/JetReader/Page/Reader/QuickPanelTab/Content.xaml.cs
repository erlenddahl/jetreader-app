using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using JetReader.BookLoaders;
using JetReader.Service;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Reader.QuickPanelTab {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Content : StackLayout {


        public event EventHandler<EbookChapter> OnChapterChange;

        public Content() {

            InitializeComponent();
        }

        public void SetNavigation(List<EbookChapter> items) {
            Device.BeginInvokeOnMainThread(() => {
                SetItems(items);
            });
        }

        private void SetItems(List<EbookChapter> items) {

            Items.Children.Clear();

            foreach (var item in GetItems(items)) {
                Items.Children.Add(item);
            }
        }

        private List<Label> GetItems(List<EbookChapter> items) {

            var labels = new List<Label>();

            foreach (var item in items) {
                var label = new Label {
                    StyleId = item.Href,
                    Text = item.Title,
                    Margin = new Thickness(item.Depth * 20, 0),
                    FontSize = Device.GetNamedSize(Device.RuntimePlatform == Device.Android ? NamedSize.Large : NamedSize.Medium, typeof(Label)),
                    TextColor = Color.White,
                };

                var tgr = new TapGestureRecognizer();
                tgr.Tapped += (s, e) => ClickToItem(item);
                label.GestureRecognizers.Add(tgr);

                labels.Add(label);
            }

            return labels;
        }

        private void ClickToItem(EbookChapter item) {
            OnChapterChange?.Invoke(this, item);
        }

    }
}