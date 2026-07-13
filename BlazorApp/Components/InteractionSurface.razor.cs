using BlazorApp3.Client.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using BlazorApp.Core.Model;
using BlazorApp.ViewModel;
using static BlazorApp.Components.ShapeTemplatPanel;
using BlazorApp.Service;
using BlazorApp.Core.Enum;
using BlazorApp.Core.Service;
using BlazorApp.Core.Model.SnapShots;
using BlazorApp.EntityFramework.Models;
using BlazorApp._state;
using BlazorApp.Controllers;
using Microsoft.AspNetCore.Mvc;


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
        [Inject] private EffectService EffectService { get; set; } = null!;
        [Inject] private VoiceCommandService VoiceCommand { get; set; } = null!;
        #endregion

        private InteractionController Controller { get; set; } = null!;

        public InteractionMode Mode => State.CurrentMode;

        public ElementReference SurfaceRef;

        private DotNetObjectReference<InteractionSurface>? _dotNetRef;

        [Parameter] public List<UILayoutModelBase> Layouts { get; set; } = [];
        [Parameter] public LayoutSection CurrentSection { get; set; } = new();
        [Parameter] public List<FieldTypeDefinition> FieldTypeDefinitions { get; set; } = [];
        [Parameter] public List<FieldEditDefinition> FieldEditDefinitions { get; set; } = [];
        [Parameter] public SurfaceInteractionMode SurfaceInteractionMode { get; set; }
        [Parameter] public EventCallback<UILayoutModelBase> OnLayoutAdded { get; set; }
        [Parameter] public EventCallback OnSave { get; set; }
        [Parameter] public OverlapMode OverlapMode { get; set; }
        //　音声命令の実行許可書
        public bool CanExecuteVoiceCommand() => State.CurrentMode == InteractionMode.StandBy;

        protected override void OnInitialized()
        {
            // Contoroller
            Controller = new InteractionController(
                State,
                SelectionService,
                DragService,
                ResizeService,
                EffectService,
                DialogService,
                UndoManager,
                FieldEditDefinitions,
                layout => OnLayoutAdded.InvokeAsync(layout)
            );

            // DisplayOptionの設定
            State.DisplayOption = new DisplayOption()
            {
                ColumnNumber = CurrentSection.ColumnNumber,
                RowNumber = CurrentSection.RowNumber,
                WidthPerCell = CurrentSection.WidthPerCell,
                HeightPerCell = CurrentSection.HeightPerCell,
                ScreenWidth = CurrentSection.ScreenWidth,
                ScreenHeight = CurrentSection.ScreenHeight,
            };

            // Add Event
            State.ModeChanged += OnInteractionModeChanged;

            // 音声命令
            // HACK:音声命令はコンポーネントのStateを変化させてるだけなので
            // InvokeAsync(StateHasChanged);が必要
            //VoiceCommand.Register(VoiceIntent.Undo, () =>
            //{
            //    UndoManager.Undo();
            //    InvokeAsync(StateHasChanged); // ★ UI 更新
            //});

            //VoiceCommand.Register(VoiceIntent.Redo, () =>
            //{
            //    UndoManager.Redo();
            //    InvokeAsync(StateHasChanged); // ★ UI 更新
            //});
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
            // StateへParameterの受け渡し
            State.Layouts = Layouts;
            State.ViewtSurfaceInteractionMode = SurfaceInteractionMode;
            State.OverlapMode = OverlapMode;
            State.CurrentSection = CurrentSection;
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
            State.CurrentDragMode = LayoutDragMode.Registering;
            // 選択解除
            Controller.CancelLayoutSelectionAll();
        }

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

            Controller.OnMouseDown(e.ShiftKey);
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

                // MoveEvent
                if (Controller.OnMouseMove())
                {
                    _ = RunDragLoop();
                }
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

                // DragによるFrame処理
                await Controller.UpdateDragFrame();

                _= InvokeAsync(StateHasChanged);
                await Task.Delay(16); // 60fps相当
            }
        }

        protected void OnMouseUp(MouseEventArgs e)
        {
            // 右クリック or メニュー表示中ならスキップ
            if (e.Button == 2 || Mode == InteractionMode.ContextMenu)
                // TODO Mode戻す
                return;

            if (Mode == InteractionMode.Idle)
            {
                Controller.OnClick(e);
                return;
            }

            Controller.OnMouseUp();
        }


        private void OnTextContextMenu()
        {
            Controller.OpenContextMenu();
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
                    State.CurrentDragMode = LayoutDragMode.Move;
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

        ///// <summary>
        ///// Drag可能かをLoopで監視
        ///// </summary>
        ///// <returns></returns>
        //private async Task ReadyLoop()
        //{
        //    while (!State.IsReadyForDrag)
        //    {
        //        await Task.Delay(16); // 60fps
        //    }

        //    Console.WriteLine("🔥 Drag Ready!");
        //}
    }

    public class ScrollPosition
    {
        public int scrollTop { get; set; }
        public int scrollLeft { get; set; }
    }
}
