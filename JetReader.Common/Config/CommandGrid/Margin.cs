namespace JetReader.Config.CommandGrid
{
    public class Margin
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }

        public Margin()
        {

        }

        public Margin(double all) : this(all, all, all, all)
        {

        }

        public Margin(double leftRight, double topBottom) : this(leftRight, topBottom, leftRight, topBottom)
        {

        }

        public Margin(double left, double top, double right, double bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}