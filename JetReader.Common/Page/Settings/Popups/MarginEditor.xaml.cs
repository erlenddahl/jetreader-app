using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Extensions.IntExtensions;
using JetReader.Config.CommandGrid;
using JetReader.Extensions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Settings.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MarginEditor : Rg.Plugins.Popup.Pages.PopupPage
    {
        private int _topValue;
        private int _leftValue;
        private int _rightValue;
        private int _bottomValue;
        private string _allValues;

        public int[] Values { get; set; }
        public string[] ValuesForAll { get; set; }
        private readonly Action<Margin> _onChangeAction;

        public int TopValue
        {
            get => _topValue;
            set
            {
                if (_topValue == value) return;
                _topValue = value;
                OnPropertyChanged();
            }
        }

        public int LeftValue
        {
            get => _leftValue;
            set
            {
                if (_leftValue == value) return;
                _leftValue = value;
                OnPropertyChanged();
            }
        }

        public int RightValue
        {
            get => _rightValue;
            set
            {
                if (_rightValue == value) return;
                _rightValue = value;
                OnPropertyChanged();
            }
        }

        public int BottomValue
        {
            get => _bottomValue;
            set
            {
                if (_bottomValue == value) return;
                _bottomValue = value;
                OnPropertyChanged();
            }
        }

        public string AllValues
        {
            get => _allValues;
            set
            {
                _allValues = ValuesForAll.First();

                if (int.TryParse(value, out var v))
                    TopValue = BottomValue = LeftValue = RightValue = v;
                OnPropertyChanged();
            }
        }

        public MarginEditor(Action<Margin> onChangeAction)
        {
            _onChangeAction = onChangeAction;

            Values = 0.To(100).ToArray();
            ValuesForAll = new[] {"Pick"}.Concat(Values.Select(p => p.ToString())).ToArray();
            _allValues = ValuesForAll.First();

            InitializeComponent();
        }

        public void SetMargins(Margin margin)
        {
            TopValue = (int)margin.Top;
            LeftValue = (int)margin.Left;
            RightValue = (int)margin.Right;
            BottomValue = (int)margin.Bottom;
        }

        private void SaveButton_OnClicked(object sender, EventArgs e)
        {
            _onChangeAction(new Margin(LeftValue, TopValue, RightValue, BottomValue));
            this.Hide();
        }

        private void CancelButton_OnClicked(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}