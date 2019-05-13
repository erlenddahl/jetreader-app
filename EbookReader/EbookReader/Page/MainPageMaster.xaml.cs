using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPageMaster : ContentPage
    {
        public ListView ListView => MenuItemsListView;
        public List<MenuItem> MenuItems { get; set; }

        public MainPageMaster()
        {
            MenuItems = new List<MenuItem>(new[]
            {
                new MenuItem { Id = 0, Title = "My Books", TargetType = typeof(HomePage) },
                new MenuItem { Id = 1, Title = "Settings", TargetType = typeof(SettingsPage) },
                new MenuItem { Id = 2, Title = "About", TargetType = typeof(AboutPage) },
            });

            InitializeComponent();

            BindingContext = this;
        }
    }
}