## Login and Role Access

### Roles and default permissions

| Role | Dashboard | Core permissions (add/remove more via Role Rights) |
| --- | --- | --- |
| Admin | `/dashboards/admin` | `*` (all), plus every permission from other roles |
| Manager | `/dashboards/manager` | `Dashboard.View`, `TeamPerformance.View`, `Leads.Assign`, `Opportunities.Assign`, `Targets.Manage`, `Incentives.View`, `Escalations.Manage`, `Reports.View` |
| SalesExecutive | `/dashboards/sales` | `Leads.View`, `Leads.Update`, `Opportunities.View`, `Opportunities.Update`, `FollowUps.Manage`, `Attendance.Manage`, `Meetings.Manage`, `Incentives.View` |
| Telecaller | `/dashboards/telecaller` | `Leads.View`, `Calls.Log`, `Calls.MarkDnd`, `Calls.MarkWrongNumber`, `CallingWindows.Access` |
| Marketing | `/dashboards/marketing` | `Campaigns.Manage`, `EmailTools.Use`, `LeadSource.Configure`, `Analytics.View` |

Permissions are additive. When `RoleRights` toggles are enabled for a user’s mapped roles, they are merged into the access list. Admin’s `*` expands to include all permissions from the template set.

### Login flow

1. User submits credentials to `POST /api/security/v1/auth/login` with payload:
   ```json
   {
     "email": "user@example.com",
     "password": "P@ssw0rd!",
     "rememberMe": true
   }
   ```
2. Credentials are verified via ASP.NET Core Identity. Failed attempts return `401` with a ProblemDetails payload. Two-factor follow-ups are unchanged.
3. Roles are gathered from Identity plus `UserRoleMappings`. If no roles are assigned, the API returns `403` with `ErrorCode=RoleMissing` and message `Access denied. No role assigned. Contact Admin.`
4. An access profile is returned on success:
   ```json
   {
     "success": true,
     "roles": ["Manager"],
     "redirectUrl": "/dashboards/manager",
     "accessProfile": {
       "roles": ["Manager"],
       "permissions": ["Dashboard.View","TeamPerformance.View","Leads.Assign","Opportunities.Assign","Targets.Manage","Incentives.View","Escalations.Manage","Reports.View"],
       "dashboardPath": "/dashboards/manager",
       "welcomeMessage": "Welcome, Manager",
       "hasAssignedRole": true,
       "sessionTimeoutMinutes": 30
     }
   }
   ```
5. Login audits record timestamp, device, and location fields. Sessions slide to a 30-minute inactivity timeout.

### Session management

- Cookie and server-side session TTL: 30 minutes of inactivity (sliding).
- Session validation middleware signs users out if the persisted session id differs or when expired.

### Extensibility guidelines

- Add new roles by extending `RoleAccessService.RoleTemplates` or by seeding new `RoleMaster` rows with matching `IdentityRole` names.
- Add or toggle permissions per role via `RoleRights` without altering controllers.
- Keep `UserRoleMappings` in sync with `AspNetUserRoles` for hybrid (multiple) role assignments.
- When integrating with stored procedures, mirror the DAL contract: `usp_ValidateUser` (credential check), `usp_GetUserRole` (role and permission fetch), and `usp_LogUserLogin` (timestamp/device/location audit).

### AppSheet notes

- Use `USEREMAIL()` to identify the logged-in user, then query role slices mapped from `UserRoleMappings`.
- Build slice filters by checking the permissions returned by the login API (e.g., allow leads module when the permission list contains `Leads.View`).
- When a user has no matching role slice, show a read-only “Access denied. Contact Admin.” view.
