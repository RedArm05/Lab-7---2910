using Lab5_Elijah_Mckeehan.Components;
using Lab5_Elijah_Mckeehan.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.MapRazorComponents()
    .AddInteractiveServerRenderMode();

app.Run(); 
