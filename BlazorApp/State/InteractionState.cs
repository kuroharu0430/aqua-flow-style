using BlazorApp.Core.Model;
using BlazorApp.Core.Enum;
using BlazorApp.Session;
using BlazorApp.Service;
using BlazorApp.ViewModel;
using static BlazorApp.Components.ShapeTemplatPanel;
using BlazorApp3.Client.Pages;
using BlazorApp.EntityFramework.Models;

namespace BlazorApp._state
{
    public class InteractionState
    {
        #region Layouts
        public List<UILayoutModelBase> Layouts { get; set; } = new();

        public IEnumerable<UILayoutModelBase> VisibleLayouts =>
            Layouts.Where(layout => layout.LayoutStatus != LayoutStatus.Deleted);

        public ShapeTemplate? PendingTemplate { get; set; } = null;
        #endregion

        public InteractionMode CurrentMode { get; private set; } = InteractionMode.StandBy;

        public LayoutDragMode CurrentDragMode { get; set; } = LayoutDragMode.Move;

        public OverlapMode OverlapMode { get; set; } = OverlapMode.Push;
        public LayoutSection CurrentSection { get; set; } = new();

        public event Action<InteractionMode>? ModeChanged;

        public SurfaceInteractionMode CurrentSurfaceInteractionMode { get; set; }

        public bool IsReadyForDrag =>
            SurfaceBase != null &&
            BaseScrollArea != null &&
            ScrollState.RelativeRectBounds != null &&
            VisibleLayouts.All(l => !l.NeedsRectUpdate);


        #region ScrollSate
        public ScrollState ScrollState { get; set; } = new();

        public MousePosition? BaseScrollArea { get; set; }

        public MousePosition SurfaceBase { get; set; }
        #endregion

        public DisplayOption DisplayOption { get; set; } = null!;
        public RectBounds? SelectionRect { get; set; } = null;

        #region MousePosition
        public MousePosition PageMousePosition { get; set; }

        public MousePosition? PageMouseDownPosition { get; set; } = null;

        public MousePosition RelativeMousePosition => (MousePosition)(PageMousePosition - SurfaceBase);
        public MousePosition AbsoluteMousePosition => new MousePosition(
                RelativeMousePosition.X + ScrollState.ScrollLeft,
                RelativeMousePosition.Y + ScrollState.ScrollTop
            );

        /// <summary>
        /// ContextMenuの表示場所
        /// </summary>
        public MousePosition? ContextMenuPosition { get; set; } = null;
        #endregion

        /// <summary>
        /// マウスダウンからMoveしたかの判定　Drag or Clickの判定につかう
        /// </summary>
        public bool MoveEnough
        {
            get
            {
                if (PageMouseDownPosition == null || PageMousePosition == null)
                {
                    return false;
                }
                const int DragThreshold = 5;
                int dx = Math.Abs(PageMousePosition.X - PageMouseDownPosition.X);
                int dy = Math.Abs(PageMousePosition.Y - PageMouseDownPosition.Y);
                return dx > DragThreshold || dy > DragThreshold;
            }
        }

        public void SetMode(InteractionMode mode)
        {
            CurrentMode = mode;
            ModeChanged?.Invoke(mode);

            // Idleに入った瞬間に基準座標を保存
            if (mode == InteractionMode.Idle)
            {
                PageMouseDownPosition = PageMousePosition;
            }
        }
    }
}

public enum InteractionMode
{
    StandBy,        // 何もしてない
    Idle,           // MoseDown中
    Dragging,       // Layoutを動かしてる
    Selecting,      // 範囲選択中
    Registering,    // 新規登録モード
    ContextMenu     // 右クリックメニュー表示中
}

public enum SurfaceInteractionMode
{
    Idle,
    Selecting,
    Dragging
}

