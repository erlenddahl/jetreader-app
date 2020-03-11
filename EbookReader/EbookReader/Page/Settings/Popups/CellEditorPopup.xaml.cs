using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EbookReader.Config.CommandGrid;
using EbookReader.Extensions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace EbookReader.Page.Settings.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CellEditorPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        private readonly Action<(Frame Frame, Label Label, GridCell Cell)> _resetColorsAction;
        private readonly Action _visualizeGridAction;
        public GridCommand[] Commands { get; set; }

        public GridCommand SelectedTapCommand
        {
            get => _selectedCell.Cell?.Tap ?? GridCommand.None;
            set
            {
                if (_selectedCell.Cell == null) return;
                if (_selectedCell.Cell.Tap == value) return;
                _selectedCell.Cell.Tap = value;
                _resetColorsAction(_selectedCell);
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
                _resetColorsAction(_selectedCell);
            }
        }

        private (Frame Frame, Label Label, GridCell Cell) _selectedCell = (null, null, null);

        private List<(Frame Frame, Label Label, GridCell Cell)> _cells = new List<(Frame Frame, Label Label, GridCell Cell)>();
        private Dictionary<string, Color> _commandColor= new Dictionary<string, Color>();
        private CommandGrid _grid;

        public CellEditorPopup(Action<(Frame Frame, Label Label, GridCell Cell)> resetColorsAction, Action visualizeGridAction)
        {
            _resetColorsAction = resetColorsAction;
            _visualizeGridAction = visualizeGridAction;

            Commands = (GridCommand[]) Enum.GetValues(typeof(GridCommand));

            InitializeComponent();
        }

        public void Edit(CommandGrid grid, (Frame Frame, Label Label, GridCell Cell) cell)
        {
            this.Show();

            _grid = grid;
            _selectedCell = cell;

            OnPropertyChanged(nameof(SelectedTapCommand));
            OnPropertyChanged(nameof(SelectedPressCommand));
        }

        private void IncreaseWidth_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            _selectedCell.Cell.Weight++;
            _visualizeGridAction();
        }

        private void DecreaseWidth_Clicked(object sender, EventArgs e)
        {

            if (_selectedCell.Cell == null) return;
            if (_selectedCell.Cell.Weight > 1)
                _selectedCell.Cell.Weight--;
            _visualizeGridAction();
        }

        private void IncreaseHeight_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            _grid.GetRow(_selectedCell.Cell).Weight++;
            _visualizeGridAction();
        }

        private void DecreaseHeight_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            if (row.Weight > 1)
                row.Weight--;
            _visualizeGridAction();
        }

        private void DeleteRow_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            _grid.Rows.Remove(row);
            _visualizeGridAction();
            this.Hide();
        }

        private void AddRowAbove_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            _grid.Rows.Insert(_grid.Rows.IndexOf(row), new GridRow() {Cells = new List<GridCell>() {new GridCell(), new GridCell()}});
            _visualizeGridAction();
        }

        private void AddRowBelow_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            _grid.Rows.Insert(_grid.Rows.IndexOf(row) + 1, new GridRow() { Cells = new List<GridCell>() { new GridCell(), new GridCell() } });
            _visualizeGridAction();
        }

        private void AddCellBefore_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            row.Cells.Insert(row.Cells.IndexOf(_selectedCell.Cell), new GridCell());
            _visualizeGridAction();
        }

        private void AddCellAfter_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            row.Cells.Insert(row.Cells.IndexOf(_selectedCell.Cell) + 1, new GridCell());
            _visualizeGridAction();
        }

        private void DeleteCell_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            row.Cells.Remove(_selectedCell.Cell);
            _visualizeGridAction();
            this.Hide();
        }

        private void CellEditorPopup_OnBackgroundClicked(object sender, EventArgs e)
        {
            _resetColorsAction(_selectedCell);
        }
    }
}