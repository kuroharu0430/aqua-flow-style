using BlazorApp.Components;
using BlazorApp.Core.Service;
using BlazorApp.Core.State;
using BlazorApp.EntityFramework.Context;
using BlazorApp.Service;
using Microsoft.EntityFrameworkCore;

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

//builder.Services.AddDbContext<LayoutDbContext>(options =>
//    options.UseSqlite("Data Source=layout.db"));

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

app.Run();
