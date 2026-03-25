using BlazorApp.Core.Model;
using BlazorApp._state;
using BlazorApp.ViewModel;
using BlazorApp.Session;

namespace BlazorApp.Service
{
    /// <summary>
    /// Layouts選択を行う。範囲選択のロジックも持つ
    /// </summary>
    public class SelectionService
    {
        //protected InteractionState State { get; private set; } = null!;

        protected SelectingSession Session { get; private set; } = null!;

        public void setSession(SelectingSession session)
        {
            Session = session;
        }

        public RectBounds GetSelectingRect(
            MousePosition currentMouse,
            (int left, int top) currentScroll)
        {
            var start = new MousePosition(
                Session.InitialMousePosition.X + Session.InitialScrollPosition.left,
                Session.InitialMousePosition.Y + Session.InitialScrollPosition.top
            );

            var current = new MousePosition(
                currentMouse.X + currentScroll.left,
                currentMouse.Y + currentScroll.top
            );

            return RectBounds.FromTwoPoints(start, current).Normalize();
        }

        public void UpdateTempSelection(
            MousePosition currentMouse,
            (int left, int top) currentScroll)
        {
            var rect = GetSelectingRect(currentMouse, currentScroll);

            foreach (var layout in Session.VisibleLayouts)
            {
                if (layout.SelectionState == SelectionState.Selected)
                    continue;

                layout.SelectionState =
                    rect.Contains(layout.RectBounds)
                    ? SelectionState.TempSelected
                    : SelectionState.None;
            }
        }

        public RectBounds GetViewRectBounds(
            MousePosition currentMouse,
            (int left, int top) currentScroll,
            MousePosition surfaceBase,
            RectBounds scrollRect)
        {
            var offset = Session.GetOffset(currentScroll);

            var relativeStart = new MousePosition(
                Session.InitialMousePosition.X - offset.left,
                Session.InitialMousePosition.Y - offset.top
            );

            var rawBounds = RectBounds.FromTwoPoints(relativeStart, currentMouse);

            var maxRectBounds = scrollRect.Offset(surfaceBase.X, surfaceBase.Y)
                .Union((Session.InitialMousePosition.X, Session.InitialMousePosition.Y));

            return rawBounds.Clamp(maxRectBounds);
        }
    }
}
