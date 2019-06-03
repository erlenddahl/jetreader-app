using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Model.Messages {
    public class ChangeBrightnessMessage {
        public double Brightness { get; set; }

        public ChangeBrightnessMessage(double brightness)
        {
            Brightness = brightness;
        }

        /// <summary>
        /// Ensures that the brightness is a valid value (between 0.01 and 1.00).
        /// </summary>
        /// <param name="brightness"></param>
        public static double Validate(double brightness)
        {
            brightness = Math.Min(brightness, 1);
            brightness = Math.Max(brightness, 0.01); // Using a minimum of 0.01 because 0.00 seems to set it to system brightness
            return brightness;
        }
    }
}
