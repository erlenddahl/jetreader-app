using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Model.Messages {
    public class ChangesBrightnessMessage {
        public double Brightness { get; set; }

        public ChangesBrightnessMessage(double brightness)
        {
            Brightness = brightness;
        }
    }
}
