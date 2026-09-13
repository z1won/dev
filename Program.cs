using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using dev;
using dev.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient
    {
        BaseAddress = new Uri("https://api.github.com/")
    };
    client.DefaultRequestHeaders.UserAgent.ParseAdd("z1won-dev-portal");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    return client;
});
builder.Services.AddScoped<ApiTelemetry>();
builder.Services.AddScoped<GitHubApiClient>();

await builder.Build().RunAsync();
