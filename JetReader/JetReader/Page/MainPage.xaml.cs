using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage {
        public MainPage() {
            InitializeComponent();
            MasterPage.ListView.ItemSelected += ListView_ItemSelected;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
            if (!(e.SelectedItem is MenuItem item))
                return;

            LoadPage(item);
        }

        public void LoadPage(MenuItem item)
        {
            if (!Detail.Navigation.NavigationStack.Any() || Detail.Navigation.NavigationStack.Last().GetType() != item.TargetType)
            {
                var page = (Xamarin.Forms.Page)Activator.CreateInstance(item.TargetType);
                page.Title = item.Title;

                Detail.Navigation.PushAsync(page);

                if (item.TargetType == typeof(HomePage))
                {
                    foreach (var pageToRemove in Detail.Navigation.NavigationStack.Where(o => o != page).ToList())
                    {
                        Detail.Navigation.RemovePage(pageToRemove);
                    }
                }
            }

            IsPresented = false;

            MasterPage.ListView.SelectedItem = null;
        }
    }
}