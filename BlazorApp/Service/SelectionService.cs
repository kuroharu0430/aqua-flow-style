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

        /// <summary>
        /// 一時選択状態(矩形確定前)のLayoutsの状態を変更
        /// </summary>
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

        #region Layouts選択
        /// <summary>
        /// MousePositionと重なったLayoutを取得する
        /// Layoutがない場合はnullを返す
        /// </summary>
        /// <returns></returns>
        public UILayoutModelBase? GetTargetLayoutAtCusor()
        {
            // 表示されていないLayoutは選択しない
            if (!State.ScrollState.RelativeRectBounds.Offset(State.SurfaceBase.X, State.SurfaceBase.Y)
                .Contains(State.RelativeMousePosition.X, State.RelativeMousePosition.Y))
            {
                return null;
            }

            return State.VisibleLayouts.FirstOrDefault(layout =>
                layout.RectBounds.Contains(State.AbsoluteMousePosition.X, State.AbsoluteMousePosition.Y));
        }

        /// <summary>
        /// Layoutsを選択状態にする
        /// </summary>
        /// <param name="target"></param>
        public void SetSelectingLayout(UILayoutModelBase? target)
        {
            if (target != null && target.SelectionState != SelectionState.Selected)
            {
                // targetが選択状態でない場合
                foreach (var layout in State.VisibleLayouts)
                    layout.SelectionState = SelectionState.None;
                target.SelectionState = SelectionState.Selected;
            }
        }

        /// <summary>
        /// Layoutsをすべて非選択状態にする
        /// </summary>
        public void CancelLayoutSelectionAll()
        {
            foreach (var layout in State.VisibleLayouts.Where(l => l.SelectionState == SelectionState.Selected))
            {
                layout.SelectionState = SelectionState.None;
            }
        }

        /// <summary>
        /// Layoutsを全て選択状態にする
        /// </summary>
        public void SelectLayoutAll()
        {
            foreach (var layout in State.VisibleLayouts.Where(l => l.SelectionState == SelectionState.None))
            {
                layout.SelectionState = SelectionState.Selected;
            }
        }

        #endregion
    }
}
