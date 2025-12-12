using Microsoft.Maui.Controls;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace BOSGlobal.Crm.Maui;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        // Register the shared Blazor App component as the root component so the MAUI host renders the same UI.
        // The shared RCL exposes `BOSGlobal.Crm.BlazorShared.App` as the root App component.
        try
        {
            blazorWebView.RootComponents.Add(new RootComponent { ComponentType = typeof(BOSGlobal.Crm.BlazorShared.App), Selector = "#app" });
        }
        catch
        {
            // If the shared RCL isn't referenced / available at runtime, fail silently — developer will get a clearer error during testing.
        }
    }
}
