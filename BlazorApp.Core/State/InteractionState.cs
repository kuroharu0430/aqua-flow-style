using BlazorApp.Core.Model;
using BlazorApp.Core.Enum;
using BlazorApp.Core.Service;
using BlazorApp.Core.Model.SnapShots;

namespace BlazorApp.Core.State
{
    public class InteractionState
    {
        public InteractionMode CurrentMode { get; private set; } = InteractionMode.StandBy;

        public event Action<InteractionMode>? ModeChanged;

        public SurfaceInteractionMode CurrentSurfaceInteractionMode { get; set; }

        public ScrollState ScrollState { get; set; } = new();

        public MoveSession? Session { get; set; } = null;

        public SelectingSession? SelectingSession { get; private set; } = null;

        public DisplayOption DisplayOption { get; set; } = null!;

        public MousePosition PageMousePosition { get; set; }

        public MousePosition? PageMouseDownPosition { get; set; } = null;

        public MousePosition RelativeMousePosition => (MousePosition)(PageMousePosition - SurfaceBase);
        public MousePosition AbsoluteMousePosition => new MousePosition(
                RelativeMousePosition.X + ScrollState.ScrollLeft,
                RelativeMousePosition.Y + ScrollState.ScrollTop
            );

        public MousePosition SurfaceBase { get; set; }

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
            // TODO DIspose()の方が適切
            Session = null;

            return new CompositeSnapshot(snapshotList, UndoActionType.Dragged);
        }

        public CompositeSnapshot CommitStyleEdit(List<IReversible> snapshotList)
        {
            foreach(var snapshot in snapshotList)
            {
                if (snapshot is FieldValueSnapShot fieldValueSnap)
                {
                    if(fieldValueSnap.target.LayoutStatus ==  LayoutStatus.Pending)
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

        public void Record(IDraggable btn)
        {
            var snapshot = new LayoutSnapshot(
                btn,
                btn.GridBounds.DeepCopy(),
                btn.LayoutStatus
            );

            PreMoveSnapshot.Add(snapshot);

            if (!OldRecord.Any(x => x.target == btn))
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
    Dragging,       // ボタンを動かしてる
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

