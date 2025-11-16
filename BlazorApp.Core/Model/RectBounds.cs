namespace BlazorApp.Core.Model
{
    public class RectBounds
    {
        public static RectBounds Empty => new RectBounds(0, 0, 0, 0);

        public int XMin { get; set; }
        public int YMin { get; set; }
        public int XMax { get; set; }
        public int YMax { get; set; }

        public RectBounds(int xMin, int yMin, int xMax, int yMax)
        {
            XMin = xMin;
            YMin = yMin;
            XMax = xMax;
            YMax = yMax;
        }

        public int Width => XMax - XMin;
        public int Height => YMax - YMin;
        public bool IsEmpty => Width < 0 || Height < 0;

        public bool Contains(RectBounds target) =>
            target.XMin >= XMin && target.XMax <= XMax &&
            target.YMin >= YMin && target.YMax <= YMax;

        public bool Contains(int x, int y) =>
            x >= XMin && x <= XMax &&
            y >= YMin && y <= YMax;

        public bool Intersects(RectBounds other) =>
            XMax > other.XMin && XMin < other.XMax &&
            YMax > other.YMin && YMin < other.YMax;
        public RectBounds Offset(int offsetX, int offsetY)
        {
            return new RectBounds(
                XMin - offsetX,
                YMin - offsetY,
                XMax - offsetX,
                YMax - offsetY
            );
        }

        public RectBounds Union((int X, int Y) point)
        {
            var xMin = Math.Min(XMin, point.X);
            var yMin = Math.Min(YMin, point.Y);
            var xMax = Math.Max(XMax, point.X);
            var yMax = Math.Max(YMax, point.Y);

            return new RectBounds(xMin, yMin, xMax, yMax);
        }

        public RectBounds Normalize() =>
            new RectBounds(
                Math.Min(XMin, XMax),
                Math.Min(YMin, YMax),
                Math.Max(XMin, XMax),
                Math.Max(YMin, YMax)
            );
        public RectBounds Clamp(RectBounds limit)
        {
            var xMin = Math.Max(XMin, limit.XMin);
            var yMin = Math.Max(YMin, limit.YMin);
            var xMax = Math.Min(XMax, limit.XMax);
            var yMax = Math.Min(YMax, limit.YMax);

            return new RectBounds(xMin, yMin, xMax, yMax).Normalize();
        }

        public static RectBounds FromTwoPoints(MousePosition p1, MousePosition p2)
        {
            return new RectBounds(p1.X, p1.Y, p2.X, p2.Y).Normalize();
        }
    }
}