using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Config.CommandGrid;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page.Settings.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommandGridConfigPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public GridCommand[] Commands { get; set; }

        public GridCommand SelectedTapCommand
        {
            get => _selectedCell.Cell?.Tap ?? GridCommand.None;
            set
            {
                if (_selectedCell.Cell == null) return;
                if (_selectedCell.Cell.Tap == value) return;
                _selectedCell.Cell.Tap = value;
                ResetColors(_selectedCell);
                OnPropertyChanged();
            }
        }

        public GridCommand SelectedPressCommand
        {
            get => _selectedCell.Cell?.Press ?? GridCommand.None;
            set
            {
                if (_selectedCell.Cell == null) return;
                if (_selectedCell.Cell.Press == value) return;
                _selectedCell.Cell.Press = value;
                ResetColors(_selectedCell);
            }
        }

        public bool IsEditorOpen
        {
            get => _isEditorOpen;
            set
            {
                _isEditorOpen = value; 
                OnPropertyChanged();
            }
        }

        private (Frame Frame, Label Label, GridCell Cell) _selectedCell = (null, null, null);

        private List<(Frame Frame, Label Label, GridCell Cell)> _cells = new List<(Frame Frame, Label Label, GridCell Cell)>();
        private Dictionary<string, Color> _commandColor= new Dictionary<string, Color>();
        private CommandGrid _grid;

        public CommandGridConfigPopup()
        {
            Commands = (GridCommand[]) Enum.GetValues(typeof(GridCommand));
            _grid = GridConfig.DefaultGrids[0];

            InitializeComponent();

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
                IsEditorOpen = false;
                _selectedCell = (null, null, null);
                ResetColors(item);
                return;
            }

            IsEditorOpen = true;

            if (_selectedCell.Frame != null)
            {
                ResetColors(_selectedCell);
            }

            item.Frame.BorderColor = Color.Red;
            item.Frame.BackgroundColor = Color.White;

            _selectedCell = item;

            OnPropertyChanged(nameof(SelectedTapCommand));
            OnPropertyChanged(nameof(SelectedPressCommand));
        }

        private void ResetColors((Frame Frame, Label Label, GridCell Cell) item)
        {
            item.Frame.BorderColor = Color.Transparent;
            item.Frame.BackgroundColor = GetCommandColor(item.Cell);
        }


        Color[] _colors = new[]
        {
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

        private bool _isEditorOpen;

        private Color GetCommandColor(GridCell cell)
        {
            var cmd = cell.Tap + "_" + cell.Press;
            if (!_commandColor.ContainsKey(cmd)) _commandColor.Add(cmd, _colors[_commandColor.Keys.Count % _colors.Length]);
            return _commandColor[cell.Tap + "_" + cell.Press];
        }

        private void CloseEditor_Clicked(object sender, EventArgs e)
        {
            IsEditorOpen = false;
        }

        private void IncreaseWidth_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            _selectedCell.Cell.Weight++;
            VisualizeGrid();
        }

        private void DecreaseWidth_Clicked(object sender, EventArgs e)
        {

            if (_selectedCell.Cell == null) return;
            _selectedCell.Cell.Weight--;
            VisualizeGrid();
        }

        private void IncreaseHeight_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            _grid.GetRow(_selectedCell.Cell).Weight++;
            VisualizeGrid();
        }

        private void DecreaseHeight_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            _grid.GetRow(_selectedCell.Cell).Weight--;
            VisualizeGrid();
        }

        private void DeleteRow_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            _grid.Rows.Remove(row);
            VisualizeGrid();
        }

        private void AddRowAbove_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            _grid.Rows.Insert(_grid.Rows.IndexOf(row), new GridRow() {Cells = new List<GridCell>() {new GridCell(), new GridCell()}});
            VisualizeGrid();
        }

        private void AddRowBelow_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            _grid.Rows.Insert(_grid.Rows.IndexOf(row) + 1, new GridRow() { Cells = new List<GridCell>() { new GridCell(), new GridCell() } });
            VisualizeGrid();
        }

        private void AddCellBefore_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            row.Cells.Insert(row.Cells.IndexOf(_selectedCell.Cell), new GridCell());
            VisualizeGrid();
        }

        private void AddCellAfter_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            row.Cells.Insert(row.Cells.IndexOf(_selectedCell.Cell) + 1, new GridCell());
            VisualizeGrid();
        }

        private void DeleteCell_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            row.Cells.Remove(_selectedCell.Cell);
            VisualizeGrid();
        }
    }
}