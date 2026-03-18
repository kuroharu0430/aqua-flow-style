using BlazorApp.Core.Model;
using System.Drawing;

namespace BlazorApp._state
{
    public class ScrollState
    {
        /// <summary>
        /// Scrollを加味しない矩形
        /// </summary>
        public RectBounds RelativeRectBounds { get; private set; } = new RectBounds(0, 0, 0, 0);
        /// <summary>
        /// Scroll込みの矩形
        /// </summary>
        public RectBounds AbsoluteRectBounds => ToAbsolute(RelativeRectBounds);

        public int ScrollTop { get; set; } = 0;
        public int ScrollLeft { get; set; } = 0;

        public bool HasScrolled => ScrollTop != 0 || ScrollLeft != 0;

        public bool IsEmpty => RelativeRectBounds.Width == 0 || RelativeRectBounds.Height == 0;

        public void UpdateBounds(RectBounds bounds)
        {
            RelativeRectBounds = bounds;
        }
        public void UpdateScroll(int top, int left)
        {
            ScrollTop = top;
            ScrollLeft = left;
        }

        /// <summary>
        /// Scroll込みの座標に変換する
        /// </summary>
        /// <returns></returns>
        public RectBounds ToAbsolute(RectBounds relativeRectBounds)
        {
            return new RectBounds(
                relativeRectBounds.XMin + ScrollLeft,
                relativeRectBounds.YMin + ScrollTop,
                relativeRectBounds.XMax + ScrollLeft,
                relativeRectBounds.YMax + ScrollTop
            );
        }
    }
}
