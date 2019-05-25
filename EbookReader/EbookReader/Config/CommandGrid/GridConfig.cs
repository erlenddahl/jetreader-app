using System;
using System.Text;

namespace EbookReader.Config.CommandGrid
{
    public class GridConfig
    {
        public static readonly CommandGrid[] DefaultGrids = new[]
        {
            new CommandGrid()
                .Row(3)
                .Cell(tap: GridCommand.PrevPage, discrete: true)
                .Cell(tap: GridCommand.VisualizeCommandCells, press: GridCommand.OpenQuickSettings)
                .Cell(tap: GridCommand.NextPage, discrete: true)
                .Row(3)
                .Cell(tap: GridCommand.PrevPage, discrete: true)
                .Cell(tap: GridCommand.ToggleFullscreen, press: GridCommand.OpenQuickSettings)
                .Cell(tap: GridCommand.NextPage, discrete: true)
                .Row(3)
                .Cell(tap: GridCommand.PrevPage)
                .Cell(tap: GridCommand.NextPage)
                .Row(1)
                .Cell(tap: GridCommand.Sync)
                .Cell(tap: GridCommand.Backup)
                .Cell(tap: GridCommand.BookInfo)
        };
    }
}
