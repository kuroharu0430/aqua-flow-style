using BlazorApp3.Client.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using BlazorApp.Core.State;
using BlazorApp.Core.Model;
using BlazorApp.Core.Service;
using BlazorApp.ViewModel;
using static BlazorApp.Components.ShapeTemplatPanel;
using BlazorApp.Components.Dialog;
using BlazorApp.Service;
using BlazorApp.Core.Enum;
using BlazorApp.Core.Model.SnapShots;
using BlazorApp.EntityFramework.Models;
using System.Reflection.Metadata.Ecma335;
using System;

namespace BlazorApp.Components
{
    public partial class InteractionSurface : ComponentBase
    {
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private InteractionState State { get; set; } = null!;
        [Inject] private UndoManager UndoManager { get; set; } = null!;
        [Inject] private DragService DragService { get; set; } = null!;
        [Inject] private ResizeService ResizeService { get; set; } = null!;
        [Inject] private SelectionService SelectionService { get; set; } = null!;
        public InteractionMode Mode => State.CurrentMode;

        public ElementReference SurfaceRef;

        private DotNetObjectReference<InteractionSurface>? _dotNetRef;

        [Parameter] public List<UILayoutModelBase> Layouts { get; set; } = [];
        [Parameter] public LayoutSection CurrentSection { get; set; } = new();

        private IEnumerable<UILayoutModelBase> VisibleLayouts =>
            Layouts.Where(layout => layout.LayoutStatus != LayoutStatus.Deleted);

        [Parameter] public List<FieldTypeDefinition> FieldTypeDefinitions { get; set; } = [];
        [Parameter] public List<FieldEditDefinition> FieldEditDefinitions { get; set; } = [];
        [Parameter] public SurfaceInteractionMode SurfaceInteractionMode { get; set; }
        [Parameter] public EventCallback<UILayoutModelBase> OnLayoutAdded { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        public LayoutDragMode CurrentDragMode { get; set; } = LayoutDragMode.Move;

        [Parameter] public OverlapMode OverlapMode { get; set; }

        private ShapeTemplate? pendingTemplate { get; set; } = null;

        public MousePosition? BaseScrollArea { get; set; }

        public RectBounds? SelectionRect { get; set; } = null;
        public readonly record struct TrailCell(int gridX, int gridY);

        protected override void OnInitialized()
        {
            State.ModeChanged += OnInteractionModeChanged;

            ResizeService.SetState(State);
            DragService.SetState(State);
            SelectionService.SetState(State);
        }

        protected override Task OnParametersSetAsync()
        {
            State.DisplayOption = new DisplayOption()
            {
                ColumnNumber = CurrentSection.ColumnNumber,
                RowNumber = CurrentSection.RowNumber,
                WidthPerCell = CurrentSection.WidthPerCell,
                HeightPerCell = CurrentSection.HeightPerCell,
                ScreenWidth = CurrentSection.ScreenWidth,
                ScreenHeight = CurrentSection.ScreenHeight,
            };
            return base.OnParametersSetAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JS.InvokeVoidAsync("initializeSurfaceManager", SurfaceRef);
                // MousePostion
                State.SurfaceBase = await JS.InvokeAsync<MousePosition>("getClientPosition", SurfaceRef);

                // JS にこのコンポーネントの参照を渡す
                _dotNetRef = DotNetObjectReference.Create(this);
                await JS.InvokeVoidAsync("registerSurfaceShortcut", _dotNetRef);
            }
        }

        [JSInvokable]
        public Task OnSurfaceMoved(MousePosition pos)
        {
            State.SurfaceBase = pos;
            return Task.CompletedTask;
        }

        private void HandleShapeSelected(ShapeTemplate template)
        {
            // 仮オブジェクトを生成
            pendingTemplate = template;
            CurrentDragMode = LayoutDragMode.Registering;
            // 選択解除
            CancelLayoutSelectionAll();
        }

        protected MousePosition? RipplePosition { get; set; } = null;

        protected List<TrailCell> TrailCells { get; set; } = new();

        private void FireRipple()
        {
            RipplePosition = State.RelativeMousePosition;

            _ = Task.Run(async () =>
            {
                await Task.Delay(500);
                RipplePosition = null;
            });
        }

        protected async Task OnMouseDown(MouseEventArgs e)
        {
            // 座標・Scroll取得
            State.PageMousePosition = new MousePosition((int)e.PageX, (int)e.PageY);

            var (top, left) = await GetScrollOffsetAsync();
            State.ScrollState.UpdateScroll(top, left);
            State.ScrollState.UpdateBounds(await GetScrollAreaBoundsAsync());
            BaseScrollArea = await JS.InvokeAsync<MousePosition>("getRelativePositionFromManager", SurfaceRef);

            // 右クリック or メニュー表示中ならスキップ
            if (e.Button == 2)
                return;

            if (Mode == InteractionMode.ContextMenu)
            {
                // メニュー表示中にクリックされた → 外部クリックとみなして閉じる
                State.SetMode(InteractionMode.StandBy);
                return;
            }

            // RippleEffect
            FireRipple();

            // ShiftキーによるSurfaceInteractionModeの反転処理
            var interactionMode = SurfaceInteractionMode;
            if (e.ShiftKey)
            {
                // TODO Switchに変える
                if (SurfaceInteractionMode == SurfaceInteractionMode.Selecting)
                {
                    interactionMode = SurfaceInteractionMode.Dragging;
                }
                else if (SurfaceInteractionMode == SurfaceInteractionMode.Dragging)
                {
                    interactionMode = SurfaceInteractionMode.Selecting;
                }
                else
                {
                    // 何もしない
                }
            }

            // 意図だけをStateに保存（確定はMoveで）
            State.CurrentSurfaceInteractionMode = interactionMode;
            State.SetMode(InteractionMode.Idle);
        }

        protected async Task OnMouseMove(MouseEventArgs e)
        {
            // Scroll補正
            var (top, left) = await GetScrollOffsetAsync();
            State.ScrollState.UpdateScroll(top, left);
            State.PageMousePosition = new MousePosition((int)e.PageX, (int)e.PageY);

            // DragStat
            // ContextMenuの競合を避ける＋clickとの競合を避ける判定
            if (State.CurrentMode == InteractionMode.Idle && State.MoveEnough)
            {
                State.ScrollState.UpdateBounds(await GetScrollAreaBoundsAsync());

                switch (State.CurrentSurfaceInteractionMode)
                {
                    case SurfaceInteractionMode.Selecting:
                        StartSelection();
                        break;
                    case SurfaceInteractionMode.Dragging:
                        StartDrag();
                        break;
                }
            }
        }

        protected void StartDrag()
        {
            // GridにMouseがない場合はDragを開始しない
            var rect = State.ScrollState.RelativeRectBounds.Offset(State.SurfaceBase.X, State.SurfaceBase.Y);
            if (!rect.Contains(State.RelativeMousePosition.X, State.RelativeMousePosition.Y))
            {
                return;
            }
            // Grid位置取得

            var dragTarget = GetTargetLayoutAtCusor();

            if (CurrentDragMode == LayoutDragMode.Registering)
            {
                // 登録処理
                (int gridX, int gridY)  = GetPositionInGrid();

                dragTarget = new UILayoutModelBase(pendingTemplate!.Title, gridX, gridY, pendingTemplate.Type, CurrentSection.Id);
                dragTarget.LayoutStatus = LayoutStatus.Pending;
                dragTarget.SelectionState = SelectionState.Selected;
                // TemplateGost release
                pendingTemplate = null;
                OnLayoutAdded.InvokeAsync(dragTarget);
            }
            else
            {
                if (dragTarget == null)
                {
                    return;
                }
                SetSelectingLayout(dragTarget);
            }

            State.SetMode(InteractionMode.Dragging);

            if (CurrentDragMode != LayoutDragMode.Registering)
            {
                // Target内の相対位置を取得
                int relativeX = State.AbsoluteMousePosition.X - dragTarget.RectBounds.XMin;
                int relativeY = State.AbsoluteMousePosition.Y - dragTarget.RectBounds.YMin;

                const int ResizeHandleSize = 12;
                bool isResizeArea = relativeX >= dragTarget.RectBounds.Width - ResizeHandleSize &&
                                    relativeY >= dragTarget.RectBounds.Height - ResizeHandleSize;

                CurrentDragMode = isResizeArea ? LayoutDragMode.Resize : LayoutDragMode.Move;
            }
            State.StartMoveSession(dragTarget, VisibleLayouts.Cast<IDraggable>().ToList());
            _= RunDragLoop();
        }

        protected void UpdateDragPosition()
        {
            if (Mode != InteractionMode.Dragging)
                return;

            (int gridX, int gridY)  = GetPositionInGrid();

            // traileffect
            if (TrailCells.LastOrDefault() != new TrailCell(gridX, gridY))
            {
                TrailCells.Add(new TrailCell(gridX, gridY));
                _ = Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    TrailCells.Remove(new TrailCell(gridX, gridY));
                });
            }

            // DragService or ResizeService
            switch (CurrentDragMode)
            {
                case LayoutDragMode.Move:
                case LayoutDragMode.Registering:
                    DragService.TryDrag(gridX, gridY, OverlapMode);
                    break;
                case LayoutDragMode.Resize:
                    ResizeService.TryResize(gridX, gridY, OverlapMode);
                    break;
                default:
                    // 何もしない
                    break;
            }
        }

        /// <summary>
        /// Drag中にLoop更新する　※AutoScroll対策
        /// </summary>
        private async Task RunDragLoop()
        {
            while (Mode == InteractionMode.Dragging)
            {
                // Scroll更新
                var (top, left) = await GetScrollOffsetAsync();
                State.ScrollState.UpdateScroll(top, left);
                //State.PageMousePosition = new MousePosition((int)e.PageX, (int)e.PageY);

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
                _= InvokeAsync(StateHasChanged);
                await Task.Delay(16); // 60fps相当
            }
        }


        private (int gridX, int gridY) GetPositionInGrid()
        {
            // ScrollArea の左上を原点とする
            var AbsoluteMouseposition = State.AbsoluteMousePosition - BaseScrollArea;

            // Scroll補正を加えた座標に変換
            var rect = State.ScrollState.AbsoluteRectBounds.Offset(State.SurfaceBase.X, State.SurfaceBase.Y);

            // スクロールエリアの最大に
            int clampedX = Math.Clamp(AbsoluteMouseposition.X, rect.XMin, rect.XMax - 1);
            int clampedY = Math.Clamp(AbsoluteMouseposition.Y, rect.YMin, rect.YMax - 1);

            int gridX = clampedX / State.DisplayOption.WidthPerCell;
            int gridY = clampedY / State.DisplayOption.HeightPerCell;

            // 最大カラム数を超えないように制御
            return (gridX: Math.Min(gridX, State.DisplayOption.ColumnNumber-1),
                    gridY: Math.Min(gridY, State.DisplayOption.RowNumber-1));
        }

        // 範囲選択モード関係
        protected void StartSelection()
        {
            State.StartSelectingSession(VisibleLayouts.Cast<IDraggableOnMouse>().ToList());
        }

        protected async Task UpdateSelection()
        {
            if (Mode != InteractionMode.Selecting) return;

            SelectionService.UpdateTempSelection();
            SelectionRect = SelectionService.GetViewRectBounds();
        }

        protected void OnMouseUp(MouseEventArgs e)
        {
            // 右クリック or メニュー表示中ならスキップ
            if (e.Button == 2 || Mode == InteractionMode.ContextMenu)
                return;

            switch (Mode)
            {
                case InteractionMode.Idle:
                    HandleClick(e);
                    break;

                case InteractionMode.Dragging:
                case InteractionMode.Registering:
                    UndoManager.Push(State.CommitDrag());
                    break;

                case InteractionMode.Selecting:
                    ConfirmSelection();
                    break;
            }

            State.SetMode(InteractionMode.StandBy);
        }

        /// <summary>
        /// Click処理
        /// </summary>
        /// <param name="e"></param>
        private void HandleClick(MouseEventArgs e)
        {
            var target = GetTargetLayoutAtCusor();
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
                foreach (var layout in VisibleLayouts)
                {
                    layout.SelectionState = SelectionState.None;

                }
                target.SelectionState = SelectionState.Selected;
            }
        }

        /// <summary>
        /// 範囲選択の確定
        /// </summary>
        private void ConfirmSelection()
        {
            foreach (var layout in VisibleLayouts.Where(l => l.SelectionState == SelectionState.TempSelected))
                layout.SelectionState = SelectionState.Selected;
        }


        private async Task<(int Top, int Left)> GetScrollOffsetAsync()
        {
            // scrollAreaElement を JS に渡すには ElementReference を使う
            var ScrollPosition = await JS.InvokeAsync<ScrollPosition>("getScrollOffsetFromManager", SurfaceRef);
            return (ScrollPosition.scrollTop, ScrollPosition.scrollLeft);
        }

        private async Task<RectBounds> GetScrollAreaBoundsAsync()
        {
            return await JS.InvokeAsync<RectBounds>("getScrollAreaBounds", SurfaceRef);
        }

        private void OnInteractionModeChanged(InteractionMode mode)
        {
            switch (mode)
            {
                case InteractionMode.StandBy:
                    // Selecting以外のZeroPoint
                    CurrentDragMode = LayoutDragMode.Move;
                    pendingTemplate = null;
                    ContextMenuPosition = null;
                    // 仮登録状態のLayoutの始末
                    Layouts = [.. Layouts.Where(l => l.LayoutStatus != LayoutStatus.Pending)];
                    foreach (var layout in Layouts)
                    {
                        layout.InteractionPhase = InteractionPhase.Idle;
                    }
                    break;
                case InteractionMode.Selecting:
                    break;

                case InteractionMode.Dragging:
                    break;

                case InteractionMode.ContextMenu:
                    break;
            }
        }

        public void Dispose()
        {
            State.ModeChanged -= OnInteractionModeChanged;
            _dotNetRef?.Dispose();
        }

        /// <summary>
        /// ContextMenuの表示場所
        /// </summary>
        public MousePosition? ContextMenuPosition { get; set; } = null;

        private void OnTextContextMenu(MouseEventArgs e)
        {
            var target = GetTargetLayoutAtCusor();
            if (target == null) return;

            SetSelectingLayout(target);
            // メニュー表示 ＋ Mode移行
            ContextMenuPosition = State.RelativeMousePosition;
            State.SetMode(InteractionMode.ContextMenu);
        }

        private async Task EditStyle()
        {
            // ContextMenu非表示
            ContextMenuPosition = null;

            var selectedLayouts = VisibleLayouts.Where(l => l.SelectionState == SelectionState.Selected).ToList();
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
            var result = await DialogService.OpenAsync<LayoutEditDialog>(
                "レイアウト編集",
                new Dictionary<string, object>
                {
                    { nameof(LayoutEditDialog.Layouts), selectedLayouts },
                    { nameof(LayoutEditDialog.FieldEditDefinitions), FieldEditDefinitions }
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
                UndoManager.Push(State.CommitStyleEdit(snapshots));

                State.SetMode(InteractionMode.StandBy);
            }
        }

        private void DeleteLayouts()
        {
            // ContextMenu非表示
            ContextMenuPosition = null;

            var targets = VisibleLayouts
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
            UndoManager.Push(new CompositeSnapshot(snapshots, UndoActionType.Deleted));

            State.SetMode(InteractionMode.StandBy);
        }

        /// <summary>
        /// MousePositionと重なったLayoutを取得する
        /// Layoutがない場合はnullを返す
        /// </summary>
        /// <returns></returns>
        private UILayoutModelBase? GetTargetLayoutAtCusor()
        {
            // 表示されていないLayoutは選択しない
            if (!State.ScrollState.RelativeRectBounds.Offset(State.SurfaceBase.X, State.SurfaceBase.Y)
                .Contains(State.RelativeMousePosition.X, State.RelativeMousePosition.Y))
            {
                return null;
            }

            return VisibleLayouts.FirstOrDefault(layout =>
                layout.RectBounds.Contains(State.AbsoluteMousePosition.X, State.AbsoluteMousePosition.Y));
        }

        /// <summary>
        /// Layoutsを選択状態にする
        /// </summary>
        /// <param name="target"></param>
        private void SetSelectingLayout(UILayoutModelBase? target)
        {
            if (target != null && target.SelectionState != SelectionState.Selected)
            {
                // targetが選択状態でない場合
                foreach (var layout in VisibleLayouts)
                    layout.SelectionState = SelectionState.None;
                target.SelectionState = SelectionState.Selected;
            }
        }

        /// <summary>
        /// Layoutsをすべて非選択状態にする
        /// </summary>
        private void CancelLayoutSelectionAll()
        {
            foreach (var layout in VisibleLayouts.Where(l => l.SelectionState == SelectionState.Selected))
            {
                layout.SelectionState = SelectionState.None;
            }
        }

        /// <summary>
        /// Layoutsを全て選択状態にする
        /// </summary>
        private void SelectLayoutAll()
        {
            foreach (var layout in VisibleLayouts.Where(l => l.SelectionState == SelectionState.None))
            {
                layout.SelectionState = SelectionState.Selected;
            }
        }
    }

    public class ScrollPosition
    {
        public int scrollTop { get; set; }
        public int scrollLeft { get; set; }
    }
}