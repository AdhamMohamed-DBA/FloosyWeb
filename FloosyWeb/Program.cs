using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FloosyWeb;
using Blazored.LocalStorage; // 1. Diefna el maktaba hena

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 2. Diefna el satr dah 3shan el LocalStorage yeshta8al
builder.Services.AddBlazoredLocalStorage();

// --- Dief el satr dah ---
builder.Services.AddScoped<FirebaseService>();
// ------------------------

builder.Services.AddScoped<LocalizationService>();

await builder.Build().RunAsync();