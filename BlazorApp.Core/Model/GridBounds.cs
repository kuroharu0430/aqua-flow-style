namespace BlazorApp.Core.Model
{
    public class GridBounds
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        public GridBounds(int x, int y, int sizeX, int sizeY)
        {
            X = x;
            Y = y;
            SizeX = sizeX;
            SizeY = sizeY;
        }

        public int XMax => X + SizeX - 1;
        public int YMax => Y + SizeY - 1;
        public int Width => SizeX;
        public int Height => SizeY;

        public bool IsEmpty => SizeX <= 0 || SizeY <= 0;

        public bool Contains(GridBounds target) =>
            target.X >= X && target.XMax <= XMax &&
            target.Y >= Y && target.YMax <= YMax;

        public bool Contains(int x, int y) =>
            x >= X && x <= XMax &&
            y >= Y && y <= YMax;

        public bool Intersects(GridBounds other) =>
            X <= other.XMax && XMax >= other.X &&
            Y <= other.YMax && YMax >= other.Y;

        public bool EqualsExact(GridBounds other) =>
            X == other.X &&
            Y == other.Y &&
            SizeX == other.SizeX &&
            SizeY == other.SizeY;

        /// <summary>
        /// 最大行数・列数からはみ出していないかの判定
        /// </summary>
        /// <param name="maxX"></param>
        /// <param name="maxY"></param>
        /// <param name="minSizeX"></param>
        /// <param name="minSizeY"></param>
        /// <returns></returns>
        public bool IsValid(int maxX, int maxY, int minSizeX = 1, int minSizeY = 1)
        {
            if (IsEmpty || SizeX < minSizeX || SizeY < minSizeY ||
                SizeX > maxX || SizeY > maxY || X < 0 || Y < 0 ||
                XMax >= maxX || YMax >= maxY)
                return false;

            return true;
        }

        public GridBounds Union(GridBounds other)
        {
            var newX = Math.Min(X, other.X);
            var newY = Math.Min(Y, other.Y);
            var newXMax = Math.Max(X + SizeX, other.X + other.SizeX);
            var newYMax = Math.Max(Y + SizeY, other.Y + other.SizeY);

            var newWidth = newXMax - newX;
            var newHeight = newYMax - newY;

            return new GridBounds(newX, newY, newWidth, newHeight);
        }

        public static GridBounds? GetOverlap(GridBounds a, GridBounds b)
        {
            int overlapX = Math.Max(a.X, b.X);
            int overlapXMax = Math.Min(a.XMax, b.XMax);
            int overlapY = Math.Max(a.Y, b.Y);
            int overlapYMax = Math.Min(a.YMax, b.YMax);

            int overlapWidth = overlapXMax - overlapX + 1;
            int overlapHeight = overlapYMax - overlapY + 1;

            if (overlapWidth <= 0 || overlapHeight <= 0)
                return null;

            return new GridBounds(overlapX, overlapY, overlapWidth, overlapHeight);
        }

        public IEnumerable<(int x, int y)> GetCells()
        {
            for (int i = X; i <= XMax; i++)
            {
                for (int j = Y; j <= YMax; j++)
                {
                    yield return (i, j);
                }
            }
        }
        public GridBounds DeepCopy()
        {
            return new GridBounds(X, Y, SizeX, SizeY);
        }
    }
}