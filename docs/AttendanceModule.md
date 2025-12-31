## Attendance Management

### Endpoints (`/api/attendance/v1`)

- `POST /punch` — punch in/out with body:
  ```json
  { "punchType": "In", "latitude": 12.9716, "longitude": 77.5946, "locationLabel": "Customer Site" }
  ```
  Validates geo-radius and shift window; returns status, message, and flags when out-of-zone or outside shift.
- `GET /rules` — list active rules (Admin/Manager).
- `POST /rules` — create/update rule (Admin). Enforces that at least one requirement is > 0.
- `POST /override` — Admin/Manager override with mandatory reason and target date/user.

### Data model

- `AttendanceLog` — punch records with geo, shift, status, and review flags.
- `AttendanceRule` — scope-based rule settings (Role/Team/User), geo radius, hybrid checks, night shift rollover, shift alias.
- `EmployeeShift` — assigned shift window, grace minutes, weekly off pattern, night rollover flag.
- `GeoMapping` — allowed geofences (home/office/customer) with radius.
- `AttendanceOverride` — manual adjustments with reason logging.

### Business logic

- Geo validation uses Haversine distance with default 150m radius (configurable per rule).
- Shift validation considers grace minutes and optional night shift rollover.
- Rules resolve with latest active rule; hybrid checks and CRM-action checks are stubbed for future integration.
- Overrides require a reason and are role-protected (Admin/Manager).

### Background job

- `AttendanceComplianceJob` (background service) runs daily as a placeholder to recalculate compliance for missed validations; extend to cross-check CRM actions and rules.

### DAL alignment (stored procedures to integrate)

- `usp_PunchAttendance(userId, lat, long, time, type)`
- `usp_GetAttendanceRules(userId)`
- `usp_GetShiftDetails(userId)`
- `usp_LogAttendanceOverride(adminId, employeeId, date, status, reason)`
- Tables: `tbl_Attendance`, `tbl_AttendanceRules`, `tbl_EmployeeShift`, `tbl_OverrideLogs`, `tbl_GeoMappings`

### UI notes

- Enforce GPS on punch; show contextual messages for out-of-zone or outside shift.
- Capture justifications for shift exceptions; flag entries for manager review.
- For AppSheet: use `Here()` for location, punch actions with visibility rules, virtual columns for rule compliance, and color-coded exceptions.
