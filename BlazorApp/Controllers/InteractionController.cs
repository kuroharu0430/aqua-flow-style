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
using BlazorApp.Components.Dialog;
using BlazorApp.EntityFramework.Models;
using Microsoft.AspNetCore.Components.Web;

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
        private readonly DialogService _dialogService;
        private readonly UndoManager _undoManager;
        private readonly List<FieldEditDefinition> _fieldEditDefinitions;
        #region Session
        private MoveSession? _moveSession;
        private SelectingSession? _selectingSession;
        #endregion

        private readonly Action<UILayoutModelBase> _onLayoutAdded;

        #region Constructor
        public InteractionController(
            InteractionState state,
            SelectionService selectionService,
            DragService dragService,
            ResizeService resizeService,
            EffectService effectService,
            DialogService dialogService,
            UndoManager undoManager,
            List<FieldEditDefinition> fieldEditDefinitions,
            Action<UILayoutModelBase> onLayoutAdded)
        {
            _state = state;
            _selectionService = selectionService;
            _dragService = dragService;
            _resizeService = resizeService;
            _effectService = effectService;
            _dialogService = dialogService;
            _undoManager = undoManager;
            _fieldEditDefinitions = fieldEditDefinitions;
            _onLayoutAdded = onLayoutAdded;
        }
        #endregion

        // --- Surface から呼ばれるイベント群 ---
        #region MouseEvent
        public void OnMouseDown(bool shiftKey)
        {
            // RippleEffect
            _effectService.FireRipple(_state.RelativeMousePosition);

            // Shift によるモード反転
            var mode = _state.ViewtSurfaceInteractionMode;
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

            // Sessionを破棄する
            _moveSession = null;
            _selectingSession = null;

            // ViewのSurfaceInteractionModeに戻す
            _state.CurrentSurfaceInteractionMode = _state.ViewtSurfaceInteractionMode;
            _state.SetMode(InteractionMode.StandBy);
        }

        /// <summary>
        /// Click処理
        /// </summary>
        /// <param name="e"></param>
        public void OnClick(MouseEventArgs e)
        {
            // Clickで確定したのでStanByに戻す
            _state.SetMode(InteractionMode.StandBy);

            var target = GetTargetLayoutAtMouseDown();
            if (target == null) return;

            // Ctrlキーが押されていない場合は他を解除
            if (e.CtrlKey)
            {
                target.SelectionState =
                    target.SelectionState == SelectionState.Selected
                    ? SelectionState.None
                    : SelectionState.Selected;
            }
            else
            {
                foreach (var layout in _state.VisibleLayouts)
                {
                    layout.SelectionState = SelectionState.None;

                }
                target.SelectionState = SelectionState.Selected;
            }
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
                _state.RelativeMouseDownPosition
            );

            _selectionService.setSession(_selectingSession);
            _state.SetMode(InteractionMode.Selecting);
        }

        /// <summary>
        /// Drag(Resize)開始
        /// </summary>
        protected void StartDrag()
        {
            if (!_state.IsReadyForDrag)
                return;

            // GridにMouseがない場合はDragを開始しない
            var rect = _state.ScrollState.RelativeRectBounds.Offset(_state.SurfaceBase.X, _state.SurfaceBase.Y);
            if (!rect.Contains(_state.RelativeMouseDownPosition.X, _state.RelativeMouseDownPosition.Y))
            {
                return;
            }

            // Grid位置取得
            var dragTarget = GetTargetLayoutAtMouseDown();

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

                // Layout追加をSurfaceに通知
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
                int relativeX = _state.AbsoluteMouseDownPosition.X - dragTarget.RectBounds.XMin;
                int relativeY = _state.AbsoluteMouseDownPosition.Y - dragTarget.RectBounds.YMin;

                const int ResizeHandleSize = 12;
                bool isResizeArea = relativeX >= dragTarget.RectBounds.Width - ResizeHandleSize &&
                                    relativeY >= dragTarget.RectBounds.Height - ResizeHandleSize;

                _state.CurrentDragMode = isResizeArea ? LayoutDragMode.Resize : LayoutDragMode.Move;
            }
            StartMoveSession(dragTarget, _state.VisibleLayouts.Cast<IDraggable>().ToList());
        }

        /// <summary>
        /// 範囲選択の確定
        /// </summary>
        private void ConfirmSelection()
        {
            foreach (var layout in _state.VisibleLayouts.Where(l => l.SelectionState == SelectionState.TempSelected))
                layout.SelectionState = SelectionState.Selected;
        }

        #region UpdateView
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
        /// MouseDown 時点の座標を使って、
        /// どの Layout を掴んだか（DragTarget）を判定する。
        /// Layout が存在しない場合は null を返す。
        /// </summary>
        public UILayoutModelBase? GetTargetLayoutAtMouseDown()
        {
            // MouseDown 時点の相対座標
            var relativeDown = _state.RelativeMouseDownPosition;
            var absoluteDown = _state.AbsoluteMouseDownPosition;

            // Grid 内かチェック
            if (!_state.ScrollState.RelativeRectBounds
                .Offset(_state.SurfaceBase.X, _state.SurfaceBase.Y)
                .Contains(relativeDown.X, relativeDown.Y))
            {
                return null;
            }

            // MouseDown 時点でどの Layout を掴んだか
            return _state.VisibleLayouts.FirstOrDefault(layout =>
                layout.RectBounds.Contains(absoluteDown.X, absoluteDown.Y));
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

        #region ContextMenu
        public void OpenContextMenu()
        {
            var target = GetTargetLayoutAtMouseDown();
            if (target == null) return;

            SetSelectingLayout(target);
            _state.ContextMenuPosition = _state.RelativeMousePosition;
            _state.SetMode(InteractionMode.ContextMenu);
        }
        #endregion

        public async Task EditStyle()
        {
            // ContextMenu非表示
            _state.ContextMenuPosition = null;

            var selectedLayouts = _state.VisibleLayouts.Where(l => l.SelectionState == SelectionState.Selected).ToList();
            // Dialog開く前にSnapshot取る
            var snapshots = new List<IReversible>();
            snapshots.AddRange(selectedLayouts.Select(l => new StyleSnapshot(l, l.Style.DeepCopy(), l.WrapperStyle.DeepCopy(), l.LayoutStatus))
                .Cast<IReversible>());

            snapshots.AddRange(
                selectedLayouts
                    .SelectMany(layout => layout.FieldValues)
                    .Select(f => (IReversible)new FieldValueSnapShot((IFieldValuable)f, f.Value, f.LayoutStatus))
            );

            // Dialog開く
            var result = await _dialogService.OpenAsync<LayoutEditDialog>(
                "レイアウト編集",
                new Dictionary<string, object>
                {
                    { nameof(LayoutEditDialog.Layouts), selectedLayouts },
                    { nameof(LayoutEditDialog.FieldEditDefinitions), _fieldEditDefinitions }
                },
                new DialogOptions { }
            );

            // 編集が確定したら履歴に積む
            if (result is true)
            {
                // 新規のFieldValueはDeletedのsnapを積む
                var addedFieldValues = selectedLayouts
                    .SelectMany(layout => layout.FieldValues)
                    .Where(f => f.LayoutStatus == LayoutStatus.Pending)
                    .Select(f => (IReversible)new FieldValueSnapShot((IFieldValuable)f, f.Value, LayoutStatus.Deleted));

                snapshots.AddRange(addedFieldValues);
                // commit && UndoPush
                _undoManager.Push(CommitStyleEdit(snapshots));

                _state.SetMode(InteractionMode.StandBy);
            }
        }

        public void DeleteLayouts()
        {
            // ContextMenu非表示
            _state.ContextMenuPosition = null;

            var targets = _state.VisibleLayouts
                .Where(layout => layout.SelectionState == SelectionState.Selected).ToList();

            var snapshots = new List<IReversible>();

            foreach (var layout in targets)
            {
                // Undo用に削除前の状態を記録
                snapshots.Add(new LayoutSnapshot(
                    layout,
                    layout.GridBounds.DeepCopy(),
                    layout.LayoutStatus
                ));

                // 削除状態に変更
                layout.LayoutStatus = LayoutStatus.Deleted;
                layout.NeedsRectUpdate = true;
            }
            // Undo履歴に積む
            _undoManager.Push(new CompositeSnapshot(snapshots, UndoActionType.Deleted));

            _state.SetMode(InteractionMode.StandBy);
        }

    }
}
