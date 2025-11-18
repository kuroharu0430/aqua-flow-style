using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorApp.Core.Model;
using BlazorApp.Service;


namespace BlazorApp.Components
{

    public partial class InteractionSurface : ComponentBase
    {
        [Parameter]
        public EventCallback OnSave { get; set; }

        [JSInvokable]
        public Task HandleShortcut(string[] keys)
        {
            // TODO State.Modeで制御する必要あり？？

            // Ctrl+Shift+Z → Redo
            // Undoも発火するのでUndoよりも先
            if (keys.Contains("Ctrl") && keys.Contains("Shift") && keys.Contains("KeyZ"))
            {
                UndoManager.Redo();
            }
            // Ctrl+Z → Undo
            else if (keys.Contains("Ctrl") && keys.Contains("KeyZ"))
            {
                UndoManager.Undo();
            }
            // Escape → Cancel
            else if (keys.Contains("Escape"))
            {
                CancelLayoutSelectionAll();
            }
            // Delete → Delete
            else if (keys.Contains("Delete"))
            {
                DeleteLayouts();
            }
            // Ctrl+A → SelectAll
            else if (keys.Contains("Ctrl") && keys.Contains("KeyA"))
            {
                SelectLayoutAll();
            }
            // Ctrl+S → Save
            else if (keys.Contains("Ctrl") && keys.Contains("KeyS"))
            {
                OnSave.InvokeAsync();
            }
            else
            {
                // 何もしない
            }

            StateHasChanged();
            return Task.CompletedTask;
        }

    }
}