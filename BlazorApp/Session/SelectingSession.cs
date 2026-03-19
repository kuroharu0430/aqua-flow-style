using BlazorApp.Core.Model;

namespace BlazorApp.Session
{
    public class SelectingSession
    {
        public (int left, int top) InitialScrollPosition { get; }

        public MousePosition InitialMousePosition { get; set; }

        public List<IDraggableOnMouse> VisibleLayouts { get; set; }

        public (int left, int top) GetOffset((int left, int top) currentScrollPosition)
        {
            return (currentScrollPosition.left - InitialScrollPosition.left,
                    currentScrollPosition.top - InitialScrollPosition.top);
        }

        public SelectingSession(List<IDraggableOnMouse> visibleLayouts, (int left, int top) ScrollPosition, MousePosition mousePosition)
        {
            VisibleLayouts = visibleLayouts;
            InitialScrollPosition = ScrollPosition;
            InitialMousePosition = mousePosition;
        }
    }
}
