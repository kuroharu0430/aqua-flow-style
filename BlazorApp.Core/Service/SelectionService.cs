using BlazorApp.Core.Model;
using BlazorApp.Core.State;

namespace BlazorApp.Core.Service
{
    public class SelectionService
    {
        protected InteractionState State { get; private set; } = null!;

        public SelectingSession Session => State.SelectingSession;

        public void SetState(InteractionState state)
        {
            if (State != null)
                throw new InvalidOperationException("State has already been set.");
            State = state;
        }

        private ScrollState ScrollState => State.ScrollState;

        public RectBounds SelectingRectWithScroll => GetSelectingRectWithScroll();

        private RectBounds GetSelectingRectWithScroll()
        {
            var start = new MousePosition(
                Session.InitialMousePosition.X + Session.InitialScrollPosition.left,
                Session.InitialMousePosition.Y + Session.InitialScrollPosition.top
            );

            var current = new MousePosition(
                State.RelativeMousePosition.X + ScrollState.ScrollLeft,
                State.RelativeMousePosition.Y + ScrollState.ScrollTop
            );

            return RectBounds.FromTwoPoints(start, current).Normalize();
        }

        public void UpdateTempSelection()
        {
            // Layout選択状態のUpdate
            foreach (var layout in Session.VisibleLayouts)
            {
                // 選択中のLayoutsは変更なし
                if (layout.SelectionState == SelectionState.Selected)
                {
                    continue;
                }

                // 矩形に含まれているのは一時選択状態に
                if (SelectingRectWithScroll.Contains(layout.RectBounds))
                {
                    layout.SelectionState = SelectionState.TempSelected;
                }
                else
                {
                    layout.SelectionState = SelectionState.None;
                }
            }
        }

        public RectBounds GetViewRectBounds()
        {
            (int left, int top) offset = Session.GetOffset((ScrollState.ScrollLeft, ScrollState.ScrollTop));
            var RelativeStart = new MousePosition(
                Session.InitialMousePosition.X - offset.left,
                Session.InitialMousePosition.Y - offset.top
            );
            var rawBounds = RectBounds.FromTwoPoints(RelativeStart, State.RelativeMousePosition);

            // 矩形制限
            var scrollRect = ScrollState.RelativeRectBounds.Offset(State.SurfaceBase.X, State.SurfaceBase.Y);
            var maxRectBounds = scrollRect.Union((Session.InitialMousePosition.X, Session.InitialMousePosition.Y));
            return rawBounds.Clamp(maxRectBounds);
            //return rawBounds;
        }
    }

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
