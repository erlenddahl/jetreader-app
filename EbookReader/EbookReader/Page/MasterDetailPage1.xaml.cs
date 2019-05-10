﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page {
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterDetailPage1 : MasterDetailPage {
        public MasterDetailPage1() {
            InitializeComponent();
            MasterPage.ListView.ItemSelected += ListView_ItemSelected;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e) {
            var item = e.SelectedItem as MasterDetailPage1MenuItem;
            if (item == null)
                return;

            LoadPage(item);
        }

        public void LoadPage(MasterDetailPage1MenuItem item)
        {
            if (!Detail.Navigation.NavigationStack.Any() ||
                Detail.Navigation.NavigationStack.Last().GetType() != item.TargetType
            )
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