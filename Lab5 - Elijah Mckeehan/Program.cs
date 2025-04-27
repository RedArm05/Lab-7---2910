using Lab5_Elijah_Mckeehan.Components;
using Lab5_Elijah_Mckeehan.Shared;
using Lab5_Elijah_Mckeehan.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ILibraryService, LibraryService>();
builder.Services.AddSingleton<IMessageService, MessageService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Ensure you're using the correct namespace for App
// If App.razor exists, this should work:
app.MapRazorComponents()
    .AddInteractiveServerRenderMode();

app.Run();
