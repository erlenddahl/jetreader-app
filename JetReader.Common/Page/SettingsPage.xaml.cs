using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public List<MenuItem> MenuItems { get; set; }

        public SettingsPage()
        {
            MenuItems = new List<MenuItem>(new[]
            {
                new MenuItem { Id = 0, Title = "Reader", TargetType = typeof(Settings.Reader) },
                new MenuItem { Id = 1, Title = "Synchronization", TargetType = typeof(Settings.Synchronization) },
                new MenuItem { Id = 2, Title = "Control", TargetType = typeof(Settings.Control) },
                new MenuItem { Id = 3, Title = "Application", TargetType = typeof(Settings.Application) },
            });

            InitializeComponent();

            if (!App.HasMasterDetailPage) {
                NavigationPage.SetHasNavigationBar(this, false);
            }

            BindingContext = this;
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MenuItem;
            if (item == null)
                return;

            ListView.SelectedItem = null;

            var page = (Xamarin.Forms.Page)Activator.CreateInstance(item.TargetType);
            await (Parent as NavigationPage).PushAsync(page, false);
            page.Title = item.Title;
        }
    }
}