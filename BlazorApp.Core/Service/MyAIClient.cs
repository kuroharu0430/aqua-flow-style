using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

public class MyAIClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public MyAIClient(HttpClient http, string apiKey)
    {
        _http = http;
        _apiKey = apiKey;
    }


    public async Task<string> FixTextAsync(string input)
    {
        var request = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
            new { role = "system", content = "入力された文章を自然な日本語に翻訳して返してください。" },
            new { role = "user", content = input }
        }
        };

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions"
        );

        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");
        httpRequest.Content = JsonContent.Create(request);

        var response = await _http.SendAsync(httpRequest);
        var json = await response.Content.ReadAsStringAsync();

        // エラーならそのまま返す
        if (!response.IsSuccessStatusCode)
        {
            return $"[API Error] {json}";
        }

        using var doc = JsonDocument.Parse(json);

        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }
}
