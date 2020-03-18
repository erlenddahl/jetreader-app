using System;
using System.Collections.Generic;
using System.Linq;
using JetReader.Config.CommandGrid;
using JetReader.Extensions;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace JetReader.Page.Reader.Popups
{
    public partial class ThemePickerPopup : PopupPage
    {
        private List<Theme> _themes = new List<Theme>();

        public List<Theme> Themes
        {
            get => _themes;
            set
            {
                _themes = value;
                Redraw();
            }
        }

        public event Action<Theme> ThemeSelected;

        public ThemePickerPopup()
        {
            Themes = Theme.DefaultThemes;

            InitializeComponent();
            Redraw();
        }

        private void Redraw()
        {
            if (Container == null) return;
            Container.Children.Clear();
            foreach(var theme in Themes)
            {
                var item = new ThemeVisualizer()
                {
                    Theme = theme,
                    HeightRequest = 150,
                    WidthRequest = 150,
                    Margin = new Thickness(20),
                };


                var tap = new TapGestureRecognizer();
                tap.Tapped += async (sender, e) =>
                {
                    await this.Hide();
                    ThemeSelected?.Invoke(theme);
                };
                item.GestureRecognizers.Add(tap);
                Container.Children.Add(item);
            }
        }
    }
}