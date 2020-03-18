using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetReader.Config.CommandGrid;
using JetReader.Extensions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace JetReader.Page.Settings.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommandGridConfigPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public GridCommand[] Commands { get; set; }

        private (Frame Frame, Label Label, GridCell Cell) _selectedCell = (null, null, null);

        private readonly List<(Frame Frame, Label Label, GridCell Cell)> _cells = new List<(Frame Frame, Label Label, GridCell Cell)>();
        private readonly Dictionary<string, Color> _commandColor= new Dictionary<string, Color>();
        private readonly CommandGrid _grid;


        readonly Color[] _colors = {
            Color.FromRgba(255 / 255d, 206 / 255d, 95 / 255d, 0.85),
            Color.FromRgba(215 / 255d, 236 / 255d, 95 / 255d, 0.85),
            Color.FromRgba(255 / 255d, 176 / 255d, 95 / 255d, 0.85),
            Color.FromRgba(255 / 255d, 46 / 255d, 95 / 255d, 0.85),
            Color.FromRgba(255 / 255d, 106 / 255d, 95 / 255d, 0.85),
            Color.FromRgba(205 / 255d, 206 / 255d, 95 / 255d, 0.85),
            Color.FromRgba(155 / 255d, 206 / 255d, 95 / 255d, 0.85),
            Color.FromRgba(55 / 255d, 206 / 255d, 95 / 255d, 0.85),
            Color.FromRgba(255 / 255d, 206 / 255d, 105 / 255d, 0.85),
            Color.FromRgba(255 / 255d, 206 / 255d, 235 / 255d, 0.85)
        };

        private readonly CellEditorPopup _editorPopup;

        public CommandGridConfigPopup()
        {
            Commands = (GridCommand[]) Enum.GetValues(typeof(GridCommand));
            _grid = UserSettings.Control.CommandGrid;

            InitializeComponent();

            _editorPopup = new CellEditorPopup(this);

            VisualizeGrid();
        }

        public void VisualizeGrid()
        {
            PreviewGrid.Children.Clear();
            PreviewGrid.RowDefinitions.Clear();
            PreviewGrid.ColumnDefinitions.Clear();
            _cells.Clear();

            var selectedCell = _selectedCell.Cell;
            _selectedCell = (null, null, null);

            for (var i = 0; i < _grid.WeightSum; i++)
                PreviewGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Star });
            PreviewGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Star});

            var gridRowIndex = 0;
            foreach (var row in _grid.Rows)
            {
                var gridRow = new Grid();
                gridRow.RowDefinitions.Add(new RowDefinition() {Height = GridLength.Star});
                for (var i = 0; i < row.WeightSum; i++)
                    gridRow.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Star});

                var gridColIndex = 0;

                foreach (var cell in row.Cells)
                {
                    var cellLayout = new Frame()
                    {
                        BackgroundColor = GetCommandColor(cell),
                        Content = new Label()
                        {
                            Text = "Tap: " + cell.Tap + Environment.NewLine + "Long: " + cell.Press,
                            HorizontalTextAlignment = TextAlignment.Center,
                            VerticalTextAlignment = TextAlignment.Center
                        }
                    };

                    var current = (cellLayout, cellLayout.Content as Label, cell);
                    if (cell == selectedCell)
                        _selectedCell = current;
                    _cells.Add(current);
                    cellLayout.GestureRecognizers.Add(new TapGestureRecognizer() {Command = new Command(() => SelectCell(current))});

                    gridRow.Children.Add(cellLayout, gridColIndex, gridColIndex + cell.Weight, 0, 1);
                    gridColIndex += cell.Weight;
                }

                PreviewGrid.Children.Add(gridRow, 0, 1, gridRowIndex, gridRowIndex + row.Weight);
                gridRowIndex += row.Weight;
            }

            // Force reselection
            if (_selectedCell.Cell != null)
            {
                var cell = _selectedCell;
                _selectedCell = (null, null, null);
                SelectCell(cell);
            }
        }

        private void SelectCell((Frame Frame, Label Label, GridCell Cell) item)
        {
            if (item == _selectedCell)
            {
                _editorPopup.Hide();
                _selectedCell = (null, null, null);
                ResetColors(item);
                return;
            }

            if (_selectedCell.Frame != null)
            {
                ResetColors(_selectedCell);
            }

            item.Frame.BorderColor = Color.Red;
            item.Frame.BackgroundColor = Color.White;

            _selectedCell = item;

            _editorPopup.Edit(_grid, _selectedCell);
        }

        public void UnselectCell()
        {
            ResetColors(_selectedCell);
            _selectedCell = (null, null, null);
        }

        public void ResetColors((Frame Frame, Label Label, GridCell Cell) item)
        {
            if (item.Frame == null) return;
            item.Frame.BorderColor = Color.Transparent;
            item.Frame.BackgroundColor = GetCommandColor(item.Cell);
            item.Label.Text = "Tap: " + item.Cell.Tap + Environment.NewLine + "Long: " + item.Cell.Press;
        }

        private Color GetCommandColor(GridCell cell)
        {
            var cmd = cell.Tap + "_" + cell.Press;
            if (!_commandColor.ContainsKey(cmd)) _commandColor.Add(cmd, _colors[_commandColor.Keys.Count % _colors.Length]);
            return _commandColor[cell.Tap + "_" + cell.Press];
        }

        protected override bool OnBackButtonPressed()
        {
            SaveConfig();
            return base.OnBackButtonPressed();
        }

        private void SaveConfig()
        {
            UserSettings.Control.CommandGrid = _grid;
        }
    }
}