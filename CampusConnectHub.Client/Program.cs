using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CampusConnectHub.Client;
using CampusConnectHub.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to the API
// In development, the API runs on a different port
// In production, use the Azure App Service URL from environment variable or configuration
var apiBaseUrl = builder.HostEnvironment.IsDevelopment() 
    ? "https://localhost:7126" 
    : builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

// If ApiBaseUrl is not configured, try to construct from BaseAddress
if (!builder.HostEnvironment.IsDevelopment() && apiBaseUrl == builder.HostEnvironment.BaseAddress)
{
    // For Azure Static Web Apps, the API should be on a separate App Service
    // This should be set via environment variable or Static Web App configuration
    // Example: https://campus-connect-hub-api.azurewebsites.net
    apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://campus-connect-hub-api.azurewebsites.net";
}

builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(apiBaseUrl) 
});

// Register services
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
