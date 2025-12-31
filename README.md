# Test-for-CRM-

## Developer notes
- Avoid pasting C# or JSON snippets directly into the terminal—keep commands and code separate to prevent shell errors.
- Seeded admin credentials are read from `appsettings.Development.json` (keys under `Seed:`) or environment variables; defaults are `admin@bosglobal.local` / `Admin@12345!` if not set.
