using EbookReader.Config.CommandGrid;

namespace EbookReader.Model.Messages
{
    public class ChangeThemeMessage
    {
        public Theme Theme { get; set; }

        public ChangeThemeMessage(Theme theme)
        {
            Theme = theme;
        }
    }
}