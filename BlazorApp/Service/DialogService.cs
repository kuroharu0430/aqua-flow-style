using BlazorApp.Components;
using Microsoft.AspNetCore.Components;

namespace BlazorApp.Service
{
    public class DialogService
    {
        private TaskCompletionSource<object?>? _tcs;

        public event Action<string, Dictionary<string, object>?, DialogOptions?, RenderFragment?>? OnOpen;

        public event Action<object?>? OnClose;

        public Task<object?> OpenAsync<TComponent>(string title, Dictionary<string, object> parameters, DialogOptions? options)
        {
            _tcs = new TaskCompletionSource<object?>();

            var fragment = new RenderFragment(builder =>
            {
                builder.OpenComponent(0, typeof(TComponent));

                if (parameters != null)
                {
                    foreach (var kvp in parameters)
                    {
                        builder.AddAttribute(1, kvp.Key, kvp.Value);
                    }
                }

                builder.CloseComponent();
            });

            OnOpen?.Invoke(title, parameters ?? new(), options, fragment);

            return _tcs.Task;
        }

        public void Close(object? result = null)
        {
            _tcs?.SetResult(result);
            _tcs = null;
            OnClose?.Invoke(result);
        }

        public Task AlertAsync(string message, string title = "Alert")
        {
            var fragment = new RenderFragment(builder =>
            {
                builder.OpenElement(0, "p");
                builder.AddContent(1, message);
                builder.CloseElement();

                builder.OpenElement(2, "button");
                builder.AddAttribute(3, "class", "btn btn-primary");
                builder.AddAttribute(4, "onclick", EventCallback.Factory.Create(this, () => Close(true)));
                builder.AddContent(5, "OK");
                builder.CloseElement();
            });

            var parameters = new Dictionary<string, object>
                {
                    { "ChildContent", fragment }
                };

            _tcs = new TaskCompletionSource<object?>();
            OnOpen?.Invoke(title, null, new DialogOptions(), fragment);
            return _tcs.Task;
        }


        public Task<bool> ConfirmAsync(string message, string title = "Confirm")
        {
            var tcs = new TaskCompletionSource<bool>();

            var fragment = new RenderFragment(builder =>
            {
                builder.OpenElement(0, "p");
                builder.AddContent(1, message);
                builder.CloseElement();

                builder.OpenElement(2, "button");
                builder.AddAttribute(3, "class", "btn btn-primary");
                builder.AddAttribute(4, "onclick", EventCallback.Factory.Create(this, () => { Close(true); tcs.SetResult(true); }));
                builder.AddContent(5, "はい");
                builder.CloseElement();

                builder.OpenElement(6, "button");
                builder.AddAttribute(7, "class", "btn btn-secondary");
                builder.AddAttribute(8, "onclick", EventCallback.Factory.Create(this, () => { Close(false); tcs.SetResult(false); }));
                builder.AddContent(9, "いいえ");
                builder.CloseElement();
            });

            var parameters = new Dictionary<string, object>
                {
                    { "ChildContent", fragment }
                };


            OnOpen?.Invoke(title, null, new DialogOptions(), fragment);
            return tcs.Task;
        }
    }

    public class DialogOptions
    {
        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? Style { get; set; }
        public bool Modal { get; set; } = true;
        public bool CloseOnOutsideClick { get; set; } = false;
    }
}
