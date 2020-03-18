using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Provider {
    public static class BrightnessChangeProvider {
        public static List<string> Items =>
            new List<string> {
                BrightnessChange.Left.ToString(),
                BrightnessChange.Right.ToString(),
                BrightnessChange.None.ToString(),
            };
    }

    public enum BrightnessChange {
        Left,
        Right,
        None
    }
}
