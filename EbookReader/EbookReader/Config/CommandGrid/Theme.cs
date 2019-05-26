using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace EbookReader.Config.CommandGrid
{
    public class Theme
    {
        public static Theme DefaultTheme => DefaultThemes[1];

        public static List<Theme> DefaultThemes = new List<Theme>()
        {
            new Theme()
            {
                Name = "Dark",
                BackgroundColor = "#181819",
                ForegroundColor = "#eff2f7"
            },

            new Theme()
            {
                Name = "White",
                BackgroundColor = "#ffffff",
                ForegroundColor = "#000000"
            },

            new Theme()
            {
                Name = "Gray",
                BackgroundColor = "#b3b3b3",
                ForegroundColor = "#272727"
            },

            new Theme()
            {
                Name = "Yellow",
                BackgroundColor = "#fff9d3",
                ForegroundColor = "#704214"
            }
        };

        public string Name { get; set; } = "Default theme";

        public string BackgroundColor { get; set; } = "#ffffff";
        public string ForegroundColor { get; set; } = "#000000";
        public string BackgroundImage { get; set; } = null;

        [JsonIgnore]
        public Color BackgroundColorXamarin => Color.FromHex(BackgroundColor);
        [JsonIgnore]
        public Color ForegroundColorXamarin => Color.FromHex(ForegroundColor);
    }
}
