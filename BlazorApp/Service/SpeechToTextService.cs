using System.Net.Http.Headers;

namespace BlazorApp.Service
{
    public class SpeechToTextService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public SpeechToTextService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["OpenAI:ApiKey"]!;
        }

        public async Task<string> TranscribeAsync(List<byte[]> chunks)
        {
            using var ms = new MemoryStream();
            foreach (var c in chunks)
                ms.Write(c, 0, c.Length);

            var allBytes = ms.ToArray();

            return await TranscribeAsync(allBytes);
        }

        public async Task<string> TranscribeAsync(byte[] audioBytes)
        {
            using var content = new ByteArrayContent(audioBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");

            using var form = new MultipartFormDataContent
    {
        { content, "file", "audio.webm" },
        { new StringContent("whisper-1"), "model" }
    };

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _http.PostAsync(
                "https://api.openai.com/v1/audio/transcriptions",
                form);

            return await response.Content.ReadAsStringAsync();
        }

        //public async Task<string> TranscribeAsync(string filePath)
        //{
        //    using var fileStream = File.OpenRead(filePath);
        //    using var fileContent = new StreamContent(fileStream);
        //    fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/webm");

        //    using var form = new MultipartFormDataContent
        //        {
        //            { fileContent, "file", "recorded.webm" },
        //            { new StringContent("whisper-1"), "model" }
        //        };

        //    _http.DefaultRequestHeaders.Authorization =
        //        new AuthenticationHeaderValue("Bearer", _apiKey);

        //    var response = await _http.PostAsync(
        //        "https://api.openai.com/v1/audio/transcriptions",
        //        form);

        //    return await response.Content.ReadAsStringAsync();
        //}
    }

    public enum VoiceState
    {
        Idle,           // 待機中
        Recording,      // 録音中
        ReceivingChunk, // JS から chunk 受信中
        Saving,         // 保存中
        Transcribing    // Whisper 送信中
    }

}
