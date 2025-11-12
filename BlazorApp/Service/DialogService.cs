namespace BlazorApp.Service
{
    public class DialogService
    {
        private TaskCompletionSource<object?>? _tcs;

        public event Action<Type, string, Dictionary<string, object>, DialogOptions?>? OnOpen;

        public event Action<object?>? OnClose;

        public Task<object?> OpenAsync<TComponent>(string title, Dictionary<string, object> parameters, DialogOptions? options)
        {
            _tcs = new TaskCompletionSource<object?>();
            OnOpen?.Invoke(typeof(TComponent), title, parameters, options);
            return _tcs.Task;
        }

        public void Close(object? result = null)
        {
            _tcs?.SetResult(result);
            _tcs = null;
            OnClose?.Invoke(result);
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
