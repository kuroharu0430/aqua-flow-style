namespace BlazorApp.Core.Model
{
    public readonly record struct SelectionBounds(
        (int X, int Y) StartPoint,
        (int X, int Y) CurrentPoint
    )
    {
        public MoveDirectionX GetDirectionX()
        {
            int dx = CurrentPoint.X - StartPoint.X;
            if (dx < 0) return MoveDirectionX.Left;
            if (dx > 0) return MoveDirectionX.Right;
            return MoveDirectionX.None;
        }

        public MoveDirectionY GetDirectionY()
        {
            int dy = CurrentPoint.Y - StartPoint.Y;
            if (dy < 0) return MoveDirectionY.Up;
            if (dy > 0) return MoveDirectionY.Down;
            return MoveDirectionY.None;
        }

        public int ClampX(int offsetX, int minX, int maxX)
        {
            int rawX = StartPoint.X - offsetX;

            switch (GetDirectionX())
            {
                case MoveDirectionX.Right:
                    if (rawX < minX) return Math.Min(StartPoint.X, minX);
                    break;
                case MoveDirectionX.Left:
                    if (rawX > maxX) return Math.Max(StartPoint.X, maxX);
                    break;
            }
            return rawX;
        }
        public int ClampY(int offsetY, int minY, int maxY)
        {
            int rawY = StartPoint.Y - offsetY;

            switch (GetDirectionY())
            {
                case MoveDirectionY.Down:
                    if (rawY > maxY) return Math.Max(StartPoint.Y, maxY);
                    break;
                case MoveDirectionY.Up:
                    if (rawY < minY) return Math.Min(StartPoint.Y, minY);
                    break;
            }
            return rawY;
        }
    }

    public enum MoveDirectionX
    {
        None,
        Left,
        Right,
    }

    public enum MoveDirectionY
    {
        None,
        Up,
        Down,
    }
}
