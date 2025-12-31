# Test-for-CRM-

## Security Gateway

The Security Gateway module provides versioned APIs for CAPTCHA verification and OTP (email, SMS, WhatsApp) delivery, with provider-agnostic messaging and database-backed challenges. See `docs/SecurityGateway.md` for request/response contracts, configuration, and operational notes.

## Login and role access

Role-aware login flows, permissions, and dashboards are documented in `docs/LoginAndRoleAccess.md`. The login API returns JSON with role assignments, merged permissions, and session policy details.

## Dashboard

Dashboard widgets, role defaults, and layout/data APIs are documented in `docs/DashboardModule.md`.

## Attendance

Attendance punch, rules, shifts, and overrides are documented in `docs/AttendanceModule.md`.
