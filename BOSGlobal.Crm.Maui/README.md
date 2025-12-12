This folder holds a .NET MAUI Blazor scaffold to host the existing Blazor UI in a native shell for Android, iOS and macOS.

IMPORTANT: This is a lightweight scaffold created for repository structure and guidance. Building/running a MAUI app requires the .NET MAUI workload installed on the developer machine (Visual Studio 2022/2023 with MAUI, or dotnet workload install), and platform-specific SDKs (Android SDK, Xcode for iOS/macOS).

How to create a full MAUI Blazor app locally
1. Install the .NET MAUI workload: `dotnet workload install maui` and required platform SDKs.
2. From the repo root run: `dotnet new maui-blazor -n BOSGlobal.Crm.Maui` to create a full project (or use the scaffolding here as a starting point).
3. Update the project to reuse your Blazor components (copy Razor components / pages or reference the `BOSGlobal.Crm.Web` project where appropriate).
4. Open the MAUI project in Visual Studio and run on Android, iOS (Simulator), or macOS.

Recommended approach for this repo
- Keep the server-side Blazor Web app (BOSGlobal.Crm.Web) as the canonical web UI and backend.
- Reuse shared UI components (Razor) in the MAUI Blazor project where feasible.
- Publish/host the server backend centrally; the MAUI apps act as native clients that call the same server-side endpoints or reuse the Blazor UI in WebView.

Notes
- This scaffold intentionally contains minimal files to avoid requiring MAUI workloads in CI/build here. Use the instructions above to generate a full working MAUI project on a machine with the proper SDKs.
