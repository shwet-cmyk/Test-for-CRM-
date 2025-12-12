using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace BOSGlobal.Crm.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .ConfigureFonts(fonts => { });

    // Register Blazor WebView so MAUI app can host Razor components
    builder.Services.AddMauiBlazorWebView();
    // Provide HttpClient in MAUI for pages/components that may call HTTP endpoints.
    builder.Services.AddHttpClient();

#if DEBUG
    builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        // Register services and Blazor WebView here to reuse server/shared services as needed.

        return builder.Build();
    }
}
