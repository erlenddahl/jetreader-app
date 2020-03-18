using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using JetReader.Extensions;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemPickerPopup : PopupPage
    {
        public string Description { get; }
        private readonly IList<string> _items;
        private readonly Action<string, int> _itemClicked;

        public ItemPickerPopup(string description, IList<string> items, Action<string, int> itemClicked)
        {
            Description = description;
            _items = items;
            _itemClicked = itemClicked;
            InitializeComponent();

            MyListView.ItemsSource = _items;
        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            this.Hide();
            _itemClicked(e.Item as string, e.ItemIndex);
        }
    }
}
