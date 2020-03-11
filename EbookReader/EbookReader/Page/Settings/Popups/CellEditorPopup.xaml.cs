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
        private readonly CommandGridConfigPopup _parent;
        public GridCommand[] Commands { get; set; }

        public GridCommand SelectedTapCommand
        {
            get => _selectedCell.Cell?.Tap ?? GridCommand.None;
            set
            {
                if (_selectedCell.Cell == null) return;
                if (_selectedCell.Cell.Tap == value) return;
                _selectedCell.Cell.Tap = value;
                _parent.ResetColors(_selectedCell);
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
                _parent.ResetColors(_selectedCell);
            }
        }

        private (Frame Frame, Label Label, GridCell Cell) _selectedCell = (null, null, null);

        private CommandGrid _grid;

        public CellEditorPopup(CommandGridConfigPopup parent)
        {
            _parent = parent;

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
            _parent.VisualizeGrid();
        }

        private void DecreaseWidth_Clicked(object sender, EventArgs e)
        {

            if (_selectedCell.Cell == null) return;
            if (_selectedCell.Cell.Weight > 1)
                _selectedCell.Cell.Weight--;
            else
            {
                var row = _grid.GetRow(_selectedCell.Cell);
                foreach(var c in row.Cells)
                    if (c != _selectedCell.Cell)
                        c.Weight++;
            }
            _parent.VisualizeGrid();
        }

        private void IncreaseHeight_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            _grid.GetRow(_selectedCell.Cell).Weight++;
            _parent.VisualizeGrid();
        }

        private void DecreaseHeight_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            if (row.Weight > 1)
                row.Weight--;
            else
            {
                foreach(var r in _grid.Rows)
                    if (r != row)
                        r.Weight++;
            }
            _parent.VisualizeGrid();
        }

        private void DeleteRow_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            _grid.Rows.Remove(row);
            _parent.VisualizeGrid();
            this.Hide();
        }

        private void AddRowAbove_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            _grid.Rows.Insert(_grid.Rows.IndexOf(row), new GridRow() {Cells = new List<GridCell>() {new GridCell(), new GridCell()}});
            _parent.VisualizeGrid();
        }

        private void AddRowBelow_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            _grid.Rows.Insert(_grid.Rows.IndexOf(row) + 1, new GridRow() { Cells = new List<GridCell>() { new GridCell(), new GridCell() } });
            _parent.VisualizeGrid();
        }

        private void AddCellBefore_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            row.Cells.Insert(row.Cells.IndexOf(_selectedCell.Cell), new GridCell());
            _parent.VisualizeGrid();
        }

        private void AddCellAfter_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            row.Cells.Insert(row.Cells.IndexOf(_selectedCell.Cell) + 1, new GridCell());
            _parent.VisualizeGrid();
        }

        private void DeleteCell_Clicked(object sender, EventArgs e)
        {
            if (_selectedCell.Cell == null) return;
            var row = _grid.GetRow(_selectedCell.Cell);
            row.Cells.Remove(_selectedCell.Cell);
            _parent.VisualizeGrid();
            this.Hide();
        }

        protected override bool OnBackButtonPressed()
        {
            _parent.UnselectCell();
            return base.OnBackButtonPressed();
        }

        private void CellEditorPopup_OnBackgroundClicked(object sender, EventArgs e)
        {
            _parent.UnselectCell();
        }
    }
}