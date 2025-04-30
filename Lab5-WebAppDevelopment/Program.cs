using Lab5_WebAppDevelopment.Components;
using Lab5_WebAppDevelopment.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ILibraryService, LibraryService>();

// Add services to the container. (This uses the Razor components hosting model.)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
