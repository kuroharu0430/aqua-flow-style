using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorApp.Core.Model;
using BlazorApp.Service;


namespace BlazorApp.Components
{

    public partial class InteractionSurface : ComponentBase
    {
        private static readonly List<(string[] combo, Action<InteractionSurface> action)> Shortcuts
            = new()
            {
                (new[] { "Ctrl", "Shift", "KeyZ" }, s => s.Controller.Redo()),
                (new[] { "Ctrl", "KeyZ" }, s => s.Controller.Undo()),
                (new[] { "Escape" }, s => s.Controller.CancelLayoutSelectionAll()),
                (new[] { "Delete" }, s => s.Controller.DeleteLayouts()),
                (new[] { "Ctrl", "KeyA" }, s => s.Controller.SelectLayoutAll()),
                (new[] { "Ctrl", "KeyS" }, s => s.OnSave.InvokeAsync())
            };

        [JSInvokable]
        public Task HandleShortcut(string[] keys)
        {
            if (Mode != InteractionMode.StandBy)
            {
                return Task.CompletedTask;
            }

            foreach (var (combo, action) in Shortcuts)
            {
                // 順不同で比較（ソートしてから比較）
                if (combo.OrderBy(k => k).SequenceEqual(keys.OrderBy(k => k)))
                {
                    action(this);
                    break;
                }
            }

            StateHasChanged();
            return Task.CompletedTask;
        }
    }
}