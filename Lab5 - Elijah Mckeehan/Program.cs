using Lab5_Elijah_Mckeehan;
using Lab5_Elijah_Mckeehan.Components;
using Lab5_Elijah_Mckeehan.Services;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ILibraryService, LibraryService>();
builder.Services.AddSingleton<IMessageService, MessageService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Configure Razor Components
builder.RootComponents.Add<App>("#app");

app.Run();
