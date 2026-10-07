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
builder.Services.AddScoped<RecorderService>();

var cs = builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContextFactory<LayoutDbContext>(options =>
    options.UseSqlite(cs));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LayoutDbContext>();
    db.Database.EnsureCreated();
}

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

app.Run();
