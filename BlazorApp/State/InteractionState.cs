using BlazorApp.Core.Model;
using BlazorApp.Core.Enum;
using BlazorApp.Core.Model.SnapShots;
using BlazorApp.Service;
using BlazorApp.ViewModel;
using static BlazorApp.Components.ShapeTemplatPanel;

namespace BlazorApp.State
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

        public event Action<InteractionMode>? ModeChanged;

        public SurfaceInteractionMode CurrentSurfaceInteractionMode { get; set; }

        #region ScrollSate
        public ScrollState ScrollState { get; set; } = new();

        public MousePosition? BaseScrollArea { get; set; }

        public MousePosition SurfaceBase { get; set; }
        #endregion

        public MoveSession? Session { get; set; } = null;

        public SelectingSession? SelectingSession { get; private set; } = null;

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

        public void StartMoveSession(IDraggable dragTarget, List<IDraggable> visibleLayouts)
        {
            Session = new MoveSession()
            {
                DragTarget = dragTarget,
                AllButtons = visibleLayouts,
            };
        }

        public void StartSelectingSession(List<IDraggableOnMouse> visibleLayouts)
        {
            SelectingSession = new SelectingSession(
                visibleLayouts,
                (ScrollState.ScrollLeft, ScrollState.ScrollTop),
                RelativeMousePosition
                );
            SetMode(InteractionMode.Selecting);
        }

        public CompositeSnapshot? CommitDrag()
        {
            if (Session == null) return null;

            var snapshotList = new List<IReversible>();

            foreach (var snapshot in Session.OldRecord)
            {
                var target = snapshot.target;

                if (target.InteractionPhase == InteractionPhase.Confirmed)
                {
                    // 仮登録から配置済状態へ移行
                    if (target.LayoutStatus == LayoutStatus.Pending)
                    {
                        target.LayoutStatus = LayoutStatus.Added;
                        // Undo用　deleted履歴の作成
                        var deletedSnapshot = snapshot with { LayoutStatus = LayoutStatus.Deleted };
                        snapshotList.Add(deletedSnapshot);
                    }
                    else
                    {
                        // 変更分のsnap追加
                        snapshotList.Add(snapshot);
                    }
                }
                else if (target.InteractionPhase == InteractionPhase.Floating)
                {
                    target.GridBounds = snapshot.Bounds.DeepCopy();
                    target.NeedsRectUpdate = true;
                }
                // Idle や Restoring は無視
            }
            // TODO Dispose()の方が適切
            Session = null;

            return new CompositeSnapshot(snapshotList, UndoActionType.Dragged);
        }

        public CompositeSnapshot CommitStyleEdit(List<IReversible> snapshotList)
        {
            foreach (var snapshot in snapshotList)
            {
                if (snapshot is FieldValueSnapShot fieldValueSnap)
                {
                    if (fieldValueSnap.target.LayoutStatus ==  LayoutStatus.Pending)
                    {
                        fieldValueSnap.target.LayoutStatus = LayoutStatus.Added;
                    }
                }
            }
            return new CompositeSnapshot(snapshotList, UndoActionType.StyleEdited);
        }
    }

    public record class MoveSession
    {
        public IDraggable DragTarget { get; set; }

        public List<IDraggable>? AllButtons { get; set; } = null;

        public List<LayoutSnapshot> PreMoveSnapshot { get; } = new();
        public List<LayoutSnapshot> OldRecord { get; } = new();

        public (int X, int Y)? LastGridPosition { get; set; }
        public (int X, int Y)? ValidGridPosition { get; set; }

        public void Record(IDraggable layout)
        {
            var snapshot = new LayoutSnapshot(
                layout,
                layout.GridBounds.DeepCopy(),
                layout.LayoutStatus
            );

            PreMoveSnapshot.Add(snapshot);

            if (!OldRecord.Any(x => x.target == layout))
            {
                OldRecord.Add(snapshot); // 最初の状態だけ積む！
            }
        }

        public void Revert()
        {
            foreach (var snap in PreMoveSnapshot)
            {
                snap.Restore();
            }
            LastGridPosition = ValidGridPosition; // 座標の巻き戻しは別責務として残す
        }


        public void PromoteInteractionPhases()
        {
            foreach (var snap in PreMoveSnapshot)
            {
                snap.target.InteractionPhase = InteractionPhase.Confirmed;
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

