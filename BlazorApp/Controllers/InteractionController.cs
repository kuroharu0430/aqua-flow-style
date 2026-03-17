using BlazorApp.Core.Model.SnapShots;
using BlazorApp.Core.Model;
using BlazorApp.Service;
using BlazorApp._state;
using BlazorApp.ViewModel;
using BlazorApp.Core.Enum;
using BlazorApp3.Client.Pages;
using BlazorApp.Session;

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

        #region Session
        private MoveSession? _moveSession;

        #endregion

        private readonly Action<UILayoutModelBase> _onLayoutAdded;

        public InteractionController(
            InteractionState state,
            SelectionService selectionService,
            DragService dragService,
            ResizeService resizeService,
            EffectService effectService,
            Action<UILayoutModelBase> onLayoutAdded)
        {
            _state = state;
            _selectionService = selectionService;
            _dragService = dragService;
            _resizeService = resizeService;
            _effectService = effectService;
            _onLayoutAdded = onLayoutAdded;
        }

        // --- Surface から呼ばれるイベント群 ---

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



        // 範囲選択モード関係
        protected void StartSelection()
        {
            _state.StartSelectingSession(_state.VisibleLayouts.Cast<IDraggableOnMouse>().ToList());
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
            var dragTarget = _selectionService.GetTargetLayoutAtCusor();

            if (_state.CurrentDragMode == LayoutDragMode.Registering)
            {
                // 登録処理
                (int gridX, int gridY)  = GetPositionInGrid();

                dragTarget = new UILayoutModelBase(_state.PendingTemplate!.Title, gridX, gridY, _state.PendingTemplate.Type, CurrentSection.Id);
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
                _selectionService.SetSelectingLayout(dragTarget);
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
            //_= RunDragLoop();
        }

        public void StartMoveSession(IDraggable dragTarget, List<IDraggable> visibleLayouts)
        {
            _moveSession = new MoveSession()
            {
                DragTarget = dragTarget,
                AllButtons = visibleLayouts,
            };
        }

        /// <summary>
        /// Drag中にLoop更新する　※AutoScroll対応
        /// </summary>
        //private async Task RunDragLoop()
        //{
        //    while (Mode == InteractionMode.Dragging || Mode ==InteractionMode.Registering || Mode ==InteractionMode.Selecting)
        //    {
        //        // Scroll更新
        //        var (top, left) = await GetScrollOffsetAsync();
        //        State.ScrollState.UpdateScroll(top, left);

        //        switch (Mode)
        //        {
        //            case InteractionMode.Selecting:
        //                await UpdateSelection();
        //                break;

        //            case InteractionMode.Dragging:
        //            case InteractionMode.Registering:
        //                UpdateDragPosition();
        //                break;
        //        }
        //        _= InvokeAsync(StateHasChanged);
        //        await Task.Delay(16); // 60fps相当
        //    }
        //}

        private (int gridX, int gridY) GetPositionInGrid()
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
    }
}
