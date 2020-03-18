using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JetReader.Model.WebViewMessages
{
    public class PanEvent
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int CurrentX { get; set; }
        public int CurrentY { get; set; }
        public bool IsFinal { get; set; }

        public int DiffX => CurrentX - StartX;
        public int DiffY => CurrentY - StartY;
    }
}
