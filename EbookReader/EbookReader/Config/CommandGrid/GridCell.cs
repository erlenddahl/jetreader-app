using Newtonsoft.Json.Linq;

namespace EbookReader.Config.CommandGrid
{
    public class GridCell
    {
        public GridCommand Tap { get; set; }
        public GridCommand Press { get; set; }
        public int Weight { get; set; }
        public bool Discrete { get; set; }

        public JObject ToJson(int weightSum)
        {
            return new JObject
            {
                {"width", Weight / (double) weightSum},
                {"showText", !Discrete},
                {"tap", Tap.ToString()},
                {"press", Press.ToString()}
            };
        }
    }
}