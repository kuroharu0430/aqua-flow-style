using Microsoft.JSInterop;

namespace BlazorApp.Service
{
    public class RecorderService
    {
        private readonly IJSRuntime JS;

        public RecorderService(IJSRuntime js)
        {
            JS = js;
        }

        public Task Start() =>
            JS.InvokeVoidAsync("voiceRecord.start").AsTask();

        public Task Stop() =>
            JS.InvokeVoidAsync("voiceRecord.stop").AsTask();

        // MinimalControllerからFileを受け取る
        public async Task GetRecordedFile(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            // AI用
            var bytes = ms.ToArray();

            // 再生用
            await SaveAudioFile(file);

            // 必要なら bytes を返す
        }

        private async Task SaveAudioFile(IFormFile file)
        {
            var path = Path.Combine("wwwroot", "recorded.webm");

            using var fs = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fs);
        }

    }
}
