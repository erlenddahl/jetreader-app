using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbookReader.Books
{
    public class Position
    {
        public int Spine { get; }
        public int SpinePosition { get; }

        public Position()
        {
        }

        public Position(int spine, int spinePosition)
        {
            Spine = spine;
            SpinePosition = spinePosition;
        }

        public Position(Position position)
        {
            Spine = position.Spine;
            SpinePosition = position.SpinePosition;
        }

        public bool Equals(Position obj)
        {
            return obj != null && Spine == obj.Spine && SpinePosition == obj.SpinePosition;
        }
    }
}
