using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace JetReader.Config.StatusPanel
{
    public enum StatusPanelItem
    {
        BookTitle,
        BookAuthor,
        Battery,
        Clock,
        ChapterTitle,
        ChapterProgress,
        BookProgress,
        BookProgressPercentage,
        Position
    }

    public class StatusPanelConfig
    {
        public static readonly StatusPanelDefinition[] DefaultPanelDefinitions = new[]
        {
            new StatusPanelDefinition()
                .Top()
                .Add()
                .Add(StatusPanelItem.BookTitle, StatusPanelItem.BookAuthor)
                .Add()
                .Bottom()
                .Add(StatusPanelItem.Battery, StatusPanelItem.Clock)
                .Add(StatusPanelItem.ChapterTitle, StatusPanelItem.ChapterProgress)
                .Add(StatusPanelItem.BookProgressPercentage)
        };
    }

    public class StatusPanelDefinition
    {
        private readonly List<StatusPanelItem[]> _top = new List<StatusPanelItem[]>();
        private readonly List<StatusPanelItem[]> _bottom = new List<StatusPanelItem[]>();

        private List<StatusPanelItem[]> _current;

        public StatusPanelDefinition Top()
        {
            _current = _top;
            return this;
        }

        public StatusPanelDefinition Bottom()
        {
            _current = _bottom;
            return this;
        }

        public StatusPanelDefinition Add(params StatusPanelItem[] keys)
        {
            _current.Add(keys);
            return this;
        }

        public JObject ToJson()
        {
            var json = new JObject
            {
                {"Top", ToJson(_top)},
                {"Bottom", ToJson(_bottom)}
            };
            return json;
        }

        private JArray ToJson(List<StatusPanelItem[]> items)
        {
            return new JArray(items.Select(p =>
            {
                if (!p.Any()) return new JArray();
                return new JArray(p.Select(c => c.ToString()));
            }));
        }
    }
}
