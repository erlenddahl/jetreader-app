using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace EbookReader.Config.CommandGrid
{
    public class CommandGrid
    {
        public List<GridRow> Rows { get; set; } = new List<GridRow>();
        private GridRow _currentRow;

        public int WeightSum => Rows.Sum(p => p.Weight);

        public CommandGrid Row(int weight = 1)
        {
            _currentRow = new GridRow() {Weight = weight};
            Rows.Add(_currentRow);
            return this;
        }

        public CommandGrid Cell(int weight = 1, GridCommand tap = GridCommand.None, GridCommand press = GridCommand.None, bool discrete = false)
        {
            var cell = new GridCell() {Weight = weight, Tap = tap, Press = press, Discrete = discrete};
            _currentRow.Cells.Add(cell);
            return this;
        }

        public JArray ToJson()
        {
            return new JArray(Rows.Select(p => p.ToJson(WeightSum)));
        }
    }
}