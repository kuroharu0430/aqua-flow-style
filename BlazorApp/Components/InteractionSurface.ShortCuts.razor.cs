using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorApp.Core.Model;
using BlazorApp.Service;


namespace BlazorApp.Components
{

    public partial class InteractionSurface : ComponentBase
    {
        [JSInvokable]
        public Task HandleShortcut()
        {
            UndoManager.Undo();
            StateHasChanged();
            return Task.CompletedTask;
        }

        [JSInvokable]
        public Task HandleRedo()
        {
            UndoManager.Redo();
            StateHasChanged();
            return Task.CompletedTask;
        }

        [JSInvokable]
        public Task HandleEscape()
        {
            foreach (var layout in VisibleLayouts.Where(l => l.SelectionState == SelectionState.Selected))
            {
                layout.SelectionState = SelectionState.None;
            }
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}