using BlazorApp3.Client.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using BlazorApp.Core.Model;
using BlazorApp.ViewModel;
using static BlazorApp.Components.ShapeTemplatPanel;
using BlazorApp.Components.Dialog;
using BlazorApp.Service;
using BlazorApp.Core.Enum;
using BlazorApp.Core.Model.SnapShots;
using BlazorApp.EntityFramework.Models;
<<<<<<< HEAD
=======
using BlazorApp.State;
>>>>>>> feature/State、Surface責務分離

namespace BlazorApp.Components
{
    public partial class InteractionSurface : ComponentBase
    {
        #region Inject
        [Inject] private IJSRuntime JS { get; set; } = null!;
        [Inject] private InteractionState State { get; set; } = null!;
        [Inject] private UndoManager UndoManager { get; set; } = null!;
        [Inject] private DragService DragService { get; set; } = null!;
        [Inject] private ResizeService ResizeService { get; set; } = null!;
        [Inject] private SelectionService SelectionService { get; set; } = null!;
<<<<<<< HEAD
<<<<<<< HEAD
        [Inject] private VoiceCommandService VoiceCommand { get; set; } = null!;
        #endregion

=======
        [Inject] private EffectService EffectService { get; set; } = null!;
>>>>>>> d26a47a (SelectionService EffectService作成)
=======
        [Inject] private EffectService EffectService { get; set; } = null!;
>>>>>>> feature/State、Surface責務分離
        public InteractionMode Mode => State.CurrentMode;

        public ElementReference SurfaceRef;

        private DotNetObjectReference<InteractionSurface>? _dotNetRef;

        [Parameter] public List<UILayoutModelBase> Layouts { get; set; } = [];
        [Parameter] public LayoutSection CurrentSection { get; set; } = new();

        private IEnumerable<UILayoutModelBase> VisibleLayouts => State.VisibleLayouts;

        [Parameter] public List<FieldTypeDefinition> FieldTypeDefinitions { get; set; } = [];
        [Parameter] public List<FieldEditDefinition> FieldEditDefinitions { get; set; } = [];
        [Parameter] public SurfaceInteractionMode SurfaceInteractionMode { get; set; }
        [Parameter] public EventCallback<UILayoutModelBase> OnLayoutAdded { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        public LayoutDragMode CurrentDragMode { get; set; } = LayoutDragMode.Move;

        [Parameter] public OverlapMode OverlapMode { get; set; }


        //public readonly record struct TrailCell(int gridX, int gridY);
<<<<<<< HEAD

        //　音声命令の実行許可書
        public bool CanExecuteVoiceCommand() => State.CurrentMode == InteractionMode.StandBy;

=======
>>>>>>> feature/State、Surface責務分離

        protected override void OnInitialized()
        {
            State.ModeChanged += OnInteractionModeChanged;

            ResizeService.SetState(State);
            DragService.SetState(State);
            SelectionService.SetState(State);

            // 音声命令
            // HACK:音声命令はコンポーネントのStateを変化させてるだけなので
            // InvokeAsync(StateHasChanged);が必要
            VoiceCommand.Register(VoiceIntent.Undo, () =>
            {
                UndoManager.Undo();
                InvokeAsync(StateHasChanged); // ★ UI 更新
            });

            VoiceCommand.Register(VoiceIntent.Redo, () =>
            {
                UndoManager.Redo();
                InvokeAsync(StateHasChanged); // ★ UI 更新
            });
        }

        /// <summary>
        /// インスタンス破棄時の処理
        /// </summary>
        public void Dispose()
        {
            State.ModeChanged -= OnInteractionModeChanged;
            _dotNetRef?.Dispose();
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
            State.Layouts = Layouts;
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
            State.PendingTemplate = template;
            CurrentDragMode = LayoutDragMode.Registering;
            // 選択解除
            SelectionService.CancelLayoutSelectionAll();
        }

        //protected List<TrailCell> TrailCells { get; set; } = new();

        protected async Task OnMouseDown(MouseEventArgs e)
        {
            // 座標・Scroll取得
            State.PageMousePosition = new MousePosition((int)e.PageX, (int)e.PageY);

            var (top, left) = await GetScrollOffsetAsync();
            State.ScrollState.UpdateScroll(top, left);
            State.ScrollState.UpdateBounds(await GetScrollAreaBoundsAsync());
            State.BaseScrollArea = await JS.InvokeAsync<MousePosition>("getRelativePositionFromManager", SurfaceRef);

            // 右クリック or メニュー表示中ならスキップ
            if (e.Button == (long)MouseButton.Right)
                return;

            if (Mode == InteractionMode.ContextMenu)
            {
                // メニュー表示中にクリックされた → 外部クリックとみなして閉じる
                State.SetMode(InteractionMode.StandBy);
                return;
            }

            // RippleEffect
            EffectService.FireRipple(State.RelativeMousePosition);

            // ShiftキーによるSurfaceInteractionModeの反転処理
            var interactionMode = SurfaceInteractionMode;
            if (e.ShiftKey)
            {
                interactionMode = SurfaceInteractionMode switch
                {
                    SurfaceInteractionMode.Selecting => SurfaceInteractionMode.Dragging,
                    SurfaceInteractionMode.Dragging => SurfaceInteractionMode.Selecting,
                    _ => SurfaceInteractionMode
                };
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

            var dragTarget = SelectionService.GetTargetLayoutAtCusor();

            if (CurrentDragMode == LayoutDragMode.Registering)
            {
                // 登録処理
                (int gridX, int gridY)  = GetPositionInGrid();

                dragTarget = new UILayoutModelBase(State.PendingTemplate!.Title, gridX, gridY, State.PendingTemplate.Type, CurrentSection.Id);
                dragTarget.LayoutStatus = LayoutStatus.Pending;
                dragTarget.SelectionState = SelectionState.Selected;
                // TemplateGost release
                State.PendingTemplate = null;
                OnLayoutAdded.InvokeAsync(dragTarget);
            }
            else
            {
                if (dragTarget == null)
                {
                    return;
                }
                SelectionService.SetSelectingLayout(dragTarget);
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

            // TrailEffect
            EffectService.AddTrail(gridX, gridY);

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
        /// Drag中にLoop更新する　※AutoScroll対応
        /// </summary>
        private async Task RunDragLoop()
        {
            while (Mode == InteractionMode.Dragging || Mode ==InteractionMode.Registering || Mode ==InteractionMode.Selecting)
            {
                // Scroll更新
                var (top, left) = await GetScrollOffsetAsync();
                State.ScrollState.UpdateScroll(top, left);

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
            var AbsoluteMouseposition = State.AbsoluteMousePosition - State.BaseScrollArea;

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
            _= RunDragLoop();
        }

        protected async Task UpdateSelection()
        {
            if (Mode != InteractionMode.Selecting) return;

            SelectionService.UpdateTempSelection();
            State.SelectionRect = SelectionService.GetViewRectBounds();
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
            var target = SelectionService.GetTargetLayoutAtCusor();
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
                    State.PendingTemplate = null;
                    State.ContextMenuPosition = null;
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

<<<<<<< HEAD
        /// <summary>
        /// ContextMenuの表示場所
        /// </summary>
        public MousePosition? ContextMenuPosition { get; set; } = null;
=======
        public void Dispose()
        {
            State.ModeChanged -= OnInteractionModeChanged;
            _dotNetRef?.Dispose();
        }
<<<<<<< HEAD
>>>>>>> d26a47a (SelectionService EffectService作成)
=======
>>>>>>> feature/State、Surface責務分離

        private void OnTextContextMenu(MouseEventArgs e)
        {
            var target = SelectionService.GetTargetLayoutAtCusor();
            if (target == null) return;

            SelectionService.SetSelectingLayout(target);
            // メニュー表示 ＋ Mode移行
            State.ContextMenuPosition = State.RelativeMousePosition;
            State.SetMode(InteractionMode.ContextMenu);
        }

        private async Task EditStyle()
        {
            // ContextMenu非表示
            State.ContextMenuPosition = null;

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
            State.ContextMenuPosition = null;

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
    }

    public class ScrollPosition
    {
        public int scrollTop { get; set; }
        public int scrollLeft { get; set; }
    }
}