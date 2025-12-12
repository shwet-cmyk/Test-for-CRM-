Repository contribution and multi-target guidance

Goal
All new UI and feature work should be made available across these targets where feasible:
- Web (BOSGlobal.Crm.Web)
- Native MAUI (BOSGlobal.Crm.Maui) targeting Android, iOS and macOS

Recommended workflow
1. Create a feature branch from `main` (e.g. `feature/xyz`).
2. Make server/backend changes in `BOSGlobal.Crm.Web` and application/domain projects.
3. Add or update shared UI (Razor) components under a shared location when possible so they can be reused by both web and MAUI Blazor projects.
4. Update the MAUI project in `BOSGlobal.Crm.Maui` to consume those components (either by referencing the shared project or copying the components as needed).
5. Open a single PR that includes the Web and MAUI changes together for review. If the MAUI changes are large, create a feature branch specifically for MAUI and include a PR linking both branches.

Notes
- MAUI builds require local SDKs and platform tools — CI may need special runners/agents for Android and macOS/iOS.
- Keep secrets (DB connection strings, reCAPTCHA secrets, SMS provider keys) out of version control. Use environment variables or secret managers for deployments.
