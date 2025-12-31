## Dashboard module

### Endpoints

All routes are versioned under `/api/dashboard/v1` and require authentication.

- `GET /api/dashboard/v1/widgets` — returns the user’s dashboard layout. If no saved layout exists, defaults are loaded from role templates (Admin, Manager, SalesExecutive, Marketing) or a fallback widget.
- `POST /api/dashboard/v1/widgets` — add/update a widget configuration (layout, filters, title override). Body:
  ```json
  {
    "widgetId": "GUID",
    "title": "My Leads",
    "layoutJson": "{ \"w\":4,\"h\":2 }",
    "filtersJson": "{ \"period\":\"30d\" }",
    "orderIndex": 0
  }
  ```
- `DELETE /api/dashboard/v1/widgets/{widgetId}` — soft-removes a widget from the user layout.
- `POST /api/dashboard/v1/widgets/{widgetId}/data` — returns data for the widget (stubbed payload with caching; replace with real query layer).

### Data model

- `WidgetLibrary` (master) — key, title, category, graph type, default config/filters, data source, enabled flag.
- `UserDashboardConfig` — per-user widget layout, filters, ordering, soft-delete flag.
- `WidgetDataCache` — cached widget payloads keyed by widget + filter hash with TTL.

### Defaults by role

- Admin: leads-over-time, conversion-rate, lead-aging, attendance-compliance, geo-checkin
- Manager: leads-over-time, conversion-rate, lead-aging, funnel-stage, incentive-booster
- SalesExecutive: leads-over-time, lead-aging, followup-analysis, funnel-stage
- Marketing: leads-over-time, conversion-rate, campaign-performance, lead-source

Defaults are applied when a user has no saved layout. Permissions are expected to be enforced upstream (via RoleAccessService) before rendering gated widgets (e.g., incentive booster).

### BAL/DAL alignment

- BAL: `DashboardService` fetches saved layout, applies role defaults, enforces enabled widgets, and caches widget data. Widget data retrieval is currently stubbed; replace with DAL calls to stored procedures:
  - `usp_GetDashboardWidgets(userId)`
  - `usp_SaveWidgetConfig(userId, widgetId, config)`
  - `usp_GetWidgetData(widgetId, filters)`
- DAL tables mirrored by EF entities:
  - `tbl_WidgetLibrary` → `WidgetLibrary`
  - `tbl_UserDashboardConfig` → `UserDashboardConfig`
  - `tbl_WidgetDataCache` → `WidgetDataCache`

### UI notes

- Widgets return layout/filter JSON to align with a grid layout engine (e.g., React Grid) and support drag-drop and resize scenarios.
- Include loading states and retry prompts on data fetch failures; cross-filtering can be implemented by publishing global filters and re-querying the `data` endpoint with merged filters.
- For AppSheet, emulate widgets via grouped views and slices; connect to the `widgets` API for configuration metadata and to `data` endpoint for chart payloads (e.g., Google Charts).
