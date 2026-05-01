using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Client;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Implementations;
using Client.Services;
using Microsoft.JSInterop;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5043") });

// Add Auth services
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<IAuthClientService, AuthClientService>();
builder.Services.AddScoped<IGroupClientService, GroupClientService>();
builder.Services.AddScoped<IPostClientService, PostClientService>();
builder.Services.AddScoped<IProfileClientService, ProfileClientService>();
builder.Services.AddScoped<ICommentClientService, CommentClientService>();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<SearchStateService>();
builder.Services.AddScoped<IAdminClientService, AdminClientService>();
builder.Services.AddScoped<IMediaClientService, MediaClientService>();
builder.Services.AddAuthorizationCore();

var host = builder.Build();

var translationSvc = host.Services.GetRequiredService<ITranslationService>();
await translationSvc.InitializeAsync();

try {
    var js = host.Services.GetRequiredService<Microsoft.JSInterop.IJSRuntime>();
    var token = await js.InvokeAsync<string>("localStorage.getItem", "authToken");
    if (!string.IsNullOrEmpty(token)) {
        var authProvider = host.Services.GetRequiredService<AuthenticationStateProvider>();
        ((CustomAuthStateProvider)authProvider).MarkUserAsAuthenticated(token);
    }
} catch { }

await host.RunAsync();
