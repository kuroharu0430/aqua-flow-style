namespace BlazorApp.Core.Model
{
    public abstract class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        protected Position() { } // JS用

        protected Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        // 継承先が自分の型を返すように実装する
        protected abstract Position Create(int x, int y);

        public Position Clamp(int minX, int minY, int maxX, int maxY)
        {
            return Create(
                Math.Clamp(X, minX, maxX),
                Math.Clamp(Y, minY, maxY)
            );
        }

        public static Position operator +(Position a, Position b)
        {
            return a.Create(a.X + b.X, a.Y + b.Y);
        }

        public static Position operator -(Position a, Position b)
        {
            return a.Create(a.X - b.X, a.Y - b.Y);
        }

        public override string ToString() => $"({X}, {Y})";
    }
    public class MousePosition : Position
    {
        public MousePosition IsZero => new MousePosition(0, 0);
        protected MousePosition() { } // JS用

        public MousePosition(int x, int y) : base(x, y) { }


        protected override MousePosition Create(int x, int y) => new MousePosition(x, y);
    }

    public class GridPosition : Position
    {
        public GridPosition(int x, int y) : base(x, y) { }

        protected override GridPosition Create(int x, int y) => new GridPosition(x, y);
    }

}
