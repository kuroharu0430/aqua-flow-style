using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorApp.Core.Model;
using BlazorApp.Service;


namespace BlazorApp.Components
{

    public partial class InteractionSurface : ComponentBase
    {
        [JSInvokable]
        public Task HandleShortcut(string[] keys)
        {
            var shortcuts = new List<(string[] combo, Action action)>
            {
                (new[] { "Ctrl", "Shift", "KeyZ" }, () => UndoManager.Redo()),
                (new[] { "Ctrl", "KeyZ" }, () => UndoManager.Undo()),
                (new[] { "Escape" }, () => Controller.CancelLayoutSelectionAll()),
                (new[] { "Delete" }, () => DeleteLayouts()),
                (new[] { "Ctrl", "KeyA" }, () => Controller.SelectLayoutAll()),
                (new[] { "Ctrl", "KeyS" }, () => OnSave.InvokeAsync())
            };

            foreach (var (combo, action) in shortcuts)
            {
                // 順不同で比較（ソートしてから比較）
                if (combo.OrderBy(k => k).SequenceEqual(keys.OrderBy(k => k)))
                {
                    action.Invoke();
                    break;
                }
            }

            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}