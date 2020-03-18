using JetReader.Config.CommandGrid;

namespace JetReader.Model.Messages
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