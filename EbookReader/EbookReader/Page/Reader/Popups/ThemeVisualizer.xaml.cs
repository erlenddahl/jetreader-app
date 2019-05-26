using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Config.CommandGrid;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page.Reader.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ThemeVisualizer : StackLayout
    {
        public static readonly BindableProperty ThemeProperty = BindableProperty.Create(nameof(Theme), typeof(Theme), typeof(ThemeVisualizer));

        public Theme Theme
        {
            get => (Theme)GetValue(ThemeProperty);
            set => SetValue(ThemeProperty, value);
        }

        public ThemeVisualizer()
        {
            InitializeComponent();
        }
    }
}