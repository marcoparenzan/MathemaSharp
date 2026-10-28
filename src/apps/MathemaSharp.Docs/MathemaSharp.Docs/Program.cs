using MathemaSharp.Docs;
using MathemaSharp.Docs.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSingleton<Documentation>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error", createScopeForErrors: true); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapGet("/assets/documentation", (string path, Documentation docs) =>
{
    var file = docs.Resolve(path, true);
    if (file is null) return Results.NotFound();
    var type = Path.GetExtension(file).ToLowerInvariant() switch
    { ".png" => "image/png", ".jpg" or ".jpeg" => "image/jpeg", ".webp" => "image/webp", _ => "application/octet-stream" };
    return Results.File(file, type, fileDownloadName: type == "application/octet-stream" ? Path.GetFileName(file) : null);
});
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
