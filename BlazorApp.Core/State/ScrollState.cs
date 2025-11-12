using BlazorApp.Core.Model;
using System.Drawing;

namespace BlazorApp.Core.State
{
    public class ScrollState
    {
        public RectBounds RectBounds { get; private set; } = new RectBounds(0, 0, 0, 0);

        public int ScrollTop { get; set; } = 0;
        public int ScrollLeft { get; set; } = 0;

        public bool HasScrolled => ScrollTop != 0 || ScrollLeft != 0;

        public bool IsEmpty => RectBounds.Width == 0 || RectBounds.Height == 0;

        public void UpdateBounds(RectBounds bounds)
        {
            RectBounds = bounds;
        }
        public void UpdateScroll(int top, int left)
        {
            ScrollTop = top;
            ScrollLeft = left;
        }
    }
}
