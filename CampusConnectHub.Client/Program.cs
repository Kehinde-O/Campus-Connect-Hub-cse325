using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CampusConnectHub.Client;
using CampusConnectHub.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to the API
// Option 2: Single App Service - Frontend and Backend on same domain
var apiBaseUrl = builder.HostEnvironment.IsDevelopment() 
    ? "https://localhost:7126"  // Development: API on different port
    : builder.HostEnvironment.BaseAddress; // Production: Same domain as frontend

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseUrl) 
});

// Register services
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
