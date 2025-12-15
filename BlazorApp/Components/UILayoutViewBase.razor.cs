using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorApp.Core.Model;
using BlazorApp.Core.Styling;
using BlazorApp.ViewModel;
using static BlazorApp.Core.Styling.StyleBuilder;

namespace BlazorApp.Components
{
    public partial class UILayoutViewBase : ComponentBase
    {
        [Inject] protected IJSRuntime JS { get; set; } = default!;
        [Parameter] public UILayoutModelBase Model { get; set; } = default!;
        [Parameter] public string SurfaceId { get; set; } = default!;
        [Parameter] public string? Style { get; set; }
        [Parameter] public string? CssClass { get; set; }
        [Parameter] public int WidthPerGrid { get; set; }
        [Parameter] public int HeightPerGrid { get; set; }

        protected StyleBuilder Styles = new();
        protected StyleBuilder WrapperStyles = new();
        protected CssClassBuilder Classes = new();
        protected CssClassBuilder WrapperClasses = new();
        protected ElementReference ElementRef;


        protected override bool ShouldRender()
        {
            return true;
            //return Model.NeedsRectUpdate; // 例：ドラッグ中は描画しない
        }

        protected override void OnParametersSet()
        {
            WrapperStyles.Clear()
                  .Position(PositionType.Absolute)
                  .Left($"{Model.GridBounds.X * WidthPerGrid}px")
                  .Top($"{Model.GridBounds.Y * HeightPerGrid}px")
                  .Width($"{Model.GridBounds.SizeX * WidthPerGrid}px")
                  .Height($"{Model.GridBounds.SizeY * HeightPerGrid}px")
                  .AddRange(Model.WrapperStyle.ToDictionary());

            Styles.Clear()
                  .AddRange(Model.Style.ToDictionary())
                  .ApplyExternalStyle(Style);

            WrapperClasses.Clear()
                  .AddCase(Model.SelectionState);

            Classes.Clear()
                   .Add("layout-base")
                   .AddCase(Model.InteractionPhase)
                   .AddCase(Model.MobilityState)
                   .ApplyExternalCssClass(CssClass);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (Model.NeedsRectUpdate && ElementRef.Id != null)
            {
                Model.RectBounds = await JS.InvokeAsync<RectBounds>("getCorrectedBoundsByIdAndRef", SurfaceId, ElementRef);
                Model.NeedsRectUpdate = false;
            }
        }
    }
}