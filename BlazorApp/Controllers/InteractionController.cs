using BlazorApp.Core.Model.SnapShots;
using BlazorApp.Core.Model;
using BlazorApp.Service;
using BlazorApp._state;
using BlazorApp.ViewModel;
using BlazorApp.Core.Enum;
using BlazorApp3.Client.Pages;
using BlazorApp.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Controllers
{
    public class InteractionController
    {
        private readonly InteractionState _state;
        public InteractionMode Mode => _state.CurrentMode;
        private readonly SelectionService _selectionService;
        private readonly DragService _dragService;
        private readonly ResizeService _resizeService;
        private readonly EffectService _effectService;
        private readonly UndoManager _undoManager;

        #region Session
        private MoveSession? _moveSession;
        private SelectingSession? _selectingSession;
        #endregion

        private readonly Action<UILayoutModelBase> _onLayoutAdded;

        public InteractionController(
            InteractionState state,
            SelectionService selectionService,
            DragService dragService,
            ResizeService resizeService,
            EffectService effectService,
            UndoManager undoManager,
            Action<UILayoutModelBase> onLayoutAdded)
        {
            _state = state;
            _selectionService = selectionService;
            _dragService = dragService;
            _resizeService = resizeService;
            _effectService = effectService;
            _undoManager = undoManager;
            _onLayoutAdded = onLayoutAdded;
        }

        // --- Surface から呼ばれるイベント群 ---
        #region MouseEvent
        public void OnMouseDown(bool shiftKey)
        {
            // RippleEffect
            _effectService.FireRipple(_state.RelativeMousePosition);

            // Shift によるモード反転
            var mode = _state.CurrentSurfaceInteractionMode;
            if (shiftKey)
            {
                mode = mode switch
                {
                    SurfaceInteractionMode.Selecting => SurfaceInteractionMode.Dragging,
                    SurfaceInteractionMode.Dragging => SurfaceInteractionMode.Selecting,
                    _ => mode
                };
            }

            _state.CurrentSurfaceInteractionMode = mode;
            _state.SetMode(InteractionMode.Idle);
        }

        public bool OnMouseMove()
        {
            switch (_state.CurrentSurfaceInteractionMode)
            {
                case SurfaceInteractionMode.Selecting:
                    StartSelection();
                    return true;

                case SurfaceInteractionMode.Dragging:
                    StartDrag();
                    return true;
            }

            return false;
        }

        public void OnMouseUp()
        {
            switch (Mode)
            {
                case InteractionMode.Dragging:
                case InteractionMode.Registering:
                    _undoManager.Push(CommitDrag());
                    break;

                case InteractionMode.Selecting:
                    ConfirmSelection();
                    break;
            }

            _state.SetMode(InteractionMode.StandBy);
        }
        #endregion

        // 範囲選択モード関係
        protected void StartSelection()
        {
            StartSelectingSession(_state.VisibleLayouts.Cast<IDraggableOnMouse>().ToList());
        }

        public void StartSelectingSession(List<IDraggableOnMouse> visibleLayouts)
        {
            _selectingSession = new SelectingSession(
                visibleLayouts,
                (_state.ScrollState.ScrollLeft, _state.ScrollState.ScrollTop),
                _state.RelativeMousePosition
                );
            _state.SetMode(InteractionMode.Selecting);
        }

        protected void StartDrag()
        {
            // GridにMouseがない場合はDragを開始しない
            var rect = _state.ScrollState.RelativeRectBounds.Offset(_state.SurfaceBase.X, _state.SurfaceBase.Y);
            if (!rect.Contains(_state.RelativeMousePosition.X, _state.RelativeMousePosition.Y))
            {
                return;
            }
            // Grid位置取得
            var dragTarget = GetTargetLayoutAtCusor();

            if (_state.CurrentDragMode == LayoutDragMode.Registering)
            {
                // 登録処理
                (int gridX, int gridY)  = GetPositionInGrid();

                dragTarget = new UILayoutModelBase(_state.PendingTemplate!.Title, gridX, gridY,
                    _state.PendingTemplate.Type, _state.CurrentSection.Id);
                dragTarget.LayoutStatus = LayoutStatus.Pending;
                dragTarget.SelectionState = SelectionState.Selected;
                // TemplateGost release
                _state.PendingTemplate = null;
                _onLayoutAdded?.Invoke(dragTarget);
            }
            else
            {
                if (dragTarget == null)
                {
                    return;
                }
                SetSelectingLayout(dragTarget);
            }

            _state.SetMode(InteractionMode.Dragging);

            if (_state.CurrentDragMode != LayoutDragMode.Registering)
            {
                // Target内の相対位置を取得
                int relativeX = _state.AbsoluteMousePosition.X - dragTarget.RectBounds.XMin;
                int relativeY = _state.AbsoluteMousePosition.Y - dragTarget.RectBounds.YMin;

                const int ResizeHandleSize = 12;
                bool isResizeArea = relativeX >= dragTarget.RectBounds.Width - ResizeHandleSize &&
                                    relativeY >= dragTarget.RectBounds.Height - ResizeHandleSize;

                _state.CurrentDragMode = isResizeArea ? LayoutDragMode.Resize : LayoutDragMode.Move;
            }
            StartMoveSession(dragTarget, _state.VisibleLayouts.Cast<IDraggable>().ToList());
            // Surfaceにパス
            //_= RunDragLoop();
        }

        /// <summary>
        /// 範囲選択の確定
        /// </summary>
        private void ConfirmSelection()
        {
            foreach (var layout in _state.VisibleLayouts.Where(l => l.SelectionState == SelectionState.TempSelected))
                layout.SelectionState = SelectionState.Selected;
        }

        #region Update
        public async Task UpdateDragFrame()
        {
            switch (Mode)
            {
                case InteractionMode.Selecting:
                    await UpdateSelection();
                    break;

                case InteractionMode.Dragging:
                case InteractionMode.Registering:
                    UpdateDragPosition();
                    break;
            }
        }
        protected async Task UpdateSelection()
        {
            if (Mode != InteractionMode.Selecting) return;

            //_selectionService.UpdateTempSelection();
            //_state.SelectionRect = _selectionService.GetViewRectBounds();
            _selectionService.UpdateTempSelection(
            _state.RelativeMousePosition,
            (_state.ScrollState.ScrollLeft, _state.ScrollState.ScrollTop)
);

            _state.SelectionRect = _selectionService.GetViewRectBounds(
                _state.RelativeMousePosition,
                (_state.ScrollState.ScrollLeft, _state.ScrollState.ScrollTop),
                _state.SurfaceBase,
                _state.ScrollState.RelativeRectBounds
            );

        }

        protected void UpdateDragPosition()
        {
            if (Mode != InteractionMode.Dragging)
                return;

            (int gridX, int gridY)  = GetPositionInGrid();

            // TrailEffect
            _effectService.AddTrail(gridX, gridY);

            // DragService or ResizeService
            switch (_state.CurrentDragMode)
            {
                case LayoutDragMode.Move:
                case LayoutDragMode.Registering:
                    _dragService.TryDrag(gridX, gridY, _state.OverlapMode);
                    break;
                case LayoutDragMode.Resize:
                    _resizeService.TryResize(gridX, gridY, _state.OverlapMode);
                    break;
                default:
                    // 何もしない
                    break;
            }
        }
        #endregion

        public void StartMoveSession(IDraggable dragTarget, List<IDraggable> visibleLayouts)
        {
            _moveSession = new MoveSession()
            {
                DragTarget = dragTarget,
                AllButtons = visibleLayouts,
            };

            // ★ Drag / Resize は MoveSession を必要とする
            _dragService.SetSession(_moveSession);
            _resizeService.SetSession(_moveSession);

            // ★ InteractionService が必要とするのは Column / Row のみ
            _dragService.SetGridInfo(
                _state.DisplayOption.ColumnNumber,
                _state.DisplayOption.RowNumber
            );

            _resizeService.SetGridInfo(
                _state.DisplayOption.ColumnNumber,
                _state.DisplayOption.RowNumber
            );
        }

        #region Commit
        public CompositeSnapshot? CommitDrag()
        {
            if (_moveSession == null) return null;

            var snapshotList = new List<IReversible>();

            foreach (var snapshot in _moveSession.OldRecord)
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
            _moveSession = null;

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

        #endregion


        public (int gridX, int gridY) GetPositionInGrid()
        {
            var absoluteMouse = _state.AbsoluteMousePosition - _state.BaseScrollArea;

            var rect = _state.ScrollState.AbsoluteRectBounds
                .Offset(_state.SurfaceBase.X, _state.SurfaceBase.Y);

            int clampedX = Math.Clamp(absoluteMouse.X, rect.XMin, rect.XMax - 1);
            int clampedY = Math.Clamp(absoluteMouse.Y, rect.YMin, rect.YMax - 1);

            int gridX = clampedX / _state.DisplayOption.WidthPerCell;
            int gridY = clampedY / _state.DisplayOption.HeightPerCell;

            return (
                Math.Min(gridX, _state.DisplayOption.ColumnNumber - 1),
                Math.Min(gridY, _state.DisplayOption.RowNumber - 1)
            );
        }

        #region Layouts選択
        /// <summary>
        /// MousePositionと重なったLayoutを取得する
        /// Layoutがない場合はnullを返す
        /// </summary>
        public UILayoutModelBase? GetTargetLayoutAtCusor()
        {
            // 表示されていないLayoutは選択しない
            if (!_state.ScrollState.RelativeRectBounds
                .Offset(_state.SurfaceBase.X, _state.SurfaceBase.Y)
                .Contains(_state.RelativeMousePosition.X, _state.RelativeMousePosition.Y))
            {
                return null;
            }

            return _state.VisibleLayouts.FirstOrDefault(layout =>
                layout.RectBounds.Contains(_state.AbsoluteMousePosition.X, _state.AbsoluteMousePosition.Y));
        }

        /// <summary>
        /// Layoutsを選択状態にする
        /// </summary>
        public void SetSelectingLayout(UILayoutModelBase? target)
        {
            if (target != null && target.SelectionState != SelectionState.Selected)
            {
                foreach (var layout in _state.VisibleLayouts)
                    layout.SelectionState = SelectionState.None;

                target.SelectionState = SelectionState.Selected;
            }
        }

        /// <summary>
        /// Layoutsをすべて非選択状態にする
        /// </summary>
        public void CancelLayoutSelectionAll()
        {
            foreach (var layout in _state.VisibleLayouts.Where(l => l.SelectionState == SelectionState.Selected))
                layout.SelectionState = SelectionState.None;
        }

        /// <summary>
        /// Layoutsを全て選択状態にする
        /// </summary>
        public void SelectLayoutAll()
        {
            foreach (var layout in _state.VisibleLayouts.Where(l => l.SelectionState == SelectionState.None))
                layout.SelectionState = SelectionState.Selected;
        }
        #endregion
    }
}
