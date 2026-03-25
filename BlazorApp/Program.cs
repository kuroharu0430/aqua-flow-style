using BlazorApp.Components;
using BlazorApp.EntityFramework.Context;
using BlazorApp.Service;
using Microsoft.EntityFrameworkCore;
using BlazorApp._state;
using BlazorApp.Core.Service;
using BlazorApp.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });

builder.Services.AddScoped<DialogService>();
builder.Services.AddTransient<InteractionState>();
builder.Services.AddTransient<UndoManager>();
builder.Services.AddTransient<DragService>();
builder.Services.AddTransient<ResizeService>();
builder.Services.AddTransient<SelectionService>();
builder.Services.AddTransient<EffectService>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<VoiceCommandService>();

builder.Services.AddDbContextFactory<LayoutDbContext>(options =>
    options.UseSqlite("Data Source=layout.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ‰¹ºFile upload
app.MapPost("/upload-audio", async (HttpRequest req) =>
{
    var form = await req.ReadFormAsync();
    var file = form.Files["file"];

    if (file is null)
        return Results.BadRequest("file ‚ª‚ ‚è‚Ü‚¹‚ñ");

    var savePath = Path.Combine("wwwroot", "recorded.webm");

    using (var fs = new FileStream(savePath, FileMode.Create))
    {
        await file.CopyToAsync(fs);
    }

    return Results.Ok();
});

// API whisper ¦YOUR_API_KEYŽæ“¾‚ª•K—v
//app.MapPost("/whisper-transcribe", async () =>
//{
//    var filePath = Path.Combine("wwwroot", "recorded.webm");

//    using var http = new HttpClient();
//    http.DefaultRequestHeaders.Authorization =
//        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "YOUR_API_KEY");

//    using var form = new MultipartFormDataContent();
//    form.Add(new StreamContent(File.OpenRead(filePath)), "file", "audio.webm");
//    form.Add(new StringContent("whisper-1"), "model");

//    var response = await http.PostAsync("https://api.openai.com/v1/audio/transcriptions", form);
//    var json = await response.Content.ReadAsStringAsync();

//    return Results.Text(json, "application/json");
//});

app.Run();
