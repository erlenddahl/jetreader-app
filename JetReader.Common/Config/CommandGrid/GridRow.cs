﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JetReader.Config.CommandGrid
{
    public class GridRow
    {
        public List<GridCell> Cells { get; set; } = new List<GridCell>();
        public int Weight { get; set; } = 1;

        public int WeightSum => Cells.Sum(p => p.Weight);

        public JObject ToJson(int weightSum)
        {
            return new JObject
            {
                {"height", Weight / (double) weightSum},
                { "cells", new JArray(Cells.Select(p => p.ToJson(WeightSum)))}
            };
        }
    }
}