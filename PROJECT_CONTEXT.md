# Smart Construction Site Manager - Project Context

Use this file when starting a new Codex chat so the project can continue even if the old conversation cannot be resumed.

## Project Goal

Smart Construction Site Manager is a university C# desktop application for managing a construction site.

The app should stay realistic and simple enough for a student project. It should not become enterprise software.

Professor requirements:

- Object-oriented design
- User-friendly GUI
- Database integration
- Basic security awareness
- Testing or build verification

## Technology Scope

- Current app type: C# WinForms desktop application
- Target framework: `net10.0-windows`
- UI should stay close to the wireframe
- Prefer WinForms-friendly solutions
- Avoid advanced cloud architecture, microservices, Kubernetes, distributed systems, or overly complex enterprise features
- Database goal: SQLite

## Planned Features

- User/login management with roles: Admin, Site Manager, Worker
- Worker management
- Project management
- Task assignment and tracking
- Attendance tracking
- Material tracking
- Issue reporting
- Progress monitoring/dashboard
- Reports section is optional if it remains realistic for the semester project

## Current Code Progress

Important files:

- `SmartConstructionSiteManagement/SeedData.cs`
- `SmartConstructionSiteManagement/Program.cs`
- `SmartConstructionSiteManagement/LoginForm.cs`
- `SmartConstructionSiteManagement/LoginForm.Designer.cs`
- `SmartConstructionSiteManagement/DashboardForm.cs`
- `SmartConstructionSiteManagement/DashboardForm.DashboardPage.cs`
- `SmartConstructionSiteManagement/DashboardForm.WorkersPage.cs`
- `SmartConstructionSiteManagement/DashboardForm.ProjectsPage.cs`
- `SmartConstructionSiteManagement/DashboardForm.TasksPage.cs`
- `SmartConstructionSiteManagement/DashboardForm.AttendancePage.cs`
- `SmartConstructionSiteManagement/DashboardForm.MaterialsPage.cs`
- `SmartConstructionSiteManagement/DashboardForm.IssueReportsPage.cs`
- `SmartConstructionSiteManagement/AuthenticationService.cs`
- `SmartConstructionSiteManagement/AppDataStore.cs`
- `SmartConstructionSiteManagement/SqliteAppDataStore.cs`
- `PROJECT_REPORT.md`

Current architecture:

- `ApplicationData` is the shared in-memory model containing users, workers, projects, tasks, attendance, materials, and issue reports.
- `IAppDataStore` is the storage interface. This is useful because the UI and authentication logic do not need to know whether data comes from JSON or SQLite.
- `JsonAppDataStore` exists as temporary/simple storage.
- `SqliteAppDataStore` exists and creates SQLite tables for the main app data.
- `SeedData` creates realistic starter data for users, workers, projects, tasks, attendance, materials, and issue reports.
- `AuthenticationService` handles login and creates default users if no users exist.
- Passwords are stored as SHA-256 hashes, which is better than plain text for a student project.

## Storage Decision

JSON is temporary storage only.

The final project should use SQLite because the professor requires database integration. The storage interface should stay in place so the app can use SQLite without rewriting every page.

SQLite package is already referenced in the project:

```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="10.0.7" />
```

## Default Login Accounts

These accounts are created when the app has no users:

- Admin: `admin@site.com` / `admin123`
- Site Manager: `manager@site.com` / `manager123`
- Worker: `worker@site.com` / `worker123`

## Build And Run Commands

Open a terminal at the solution folder:

```powershell
cd "G:\Smart Construction Site Manager(App)"
```

Build:

```powershell
dotnet build SmartConstructionSiteManagement\SmartConstructionSiteManagement.csproj
```

Run:

```powershell
dotnet run --project SmartConstructionSiteManagement\SmartConstructionSiteManagement.csproj
```

If Windows locks `apphost.exe`, remove it only after closing the app:

```powershell
Remove-Item -Force "G:\Smart Construction Site Manager(App)\SmartConstructionSiteManagement\obj\Debug\net10.0-windows\apphost.exe"
```

Then build again.

## Important Workflow Note

Do not paste PowerShell output back into the terminal as a command. Only type the actual command, not lines like `Windows PowerShell`, `Copyright`, `Build succeeded`, or prompt text such as `PS G:\...>`.

## Current Concern

The VS Code Codex conversation sometimes cannot resume the next day because of a session path/resume mismatch. The project files are fine; the chat UI is the issue.

To continue in a new chat, paste this:

```text
Please read PROJECT_CONTEXT.md and continue from the project context.

Current status: the app has 20 seeded rows for each main management page, all filtered rows are shown, and add/edit dialogs now use dropdowns for existing related records. Login quick-fill/password suggestion was discussed but intentionally skipped. Continue by checking Add/Edit/Delete, search, and filter behavior across all pages.
```

## Current Seed Data

The app now has a dedicated `SeedData.cs` file. It creates:

- 3 users
- 20 workers
- 20 projects
- 20 tasks
- 20 attendance records
- 20 materials
- 20 issue reports

The seed data is used only when the app has no existing data. It should not overwrite records that were already saved in SQLite.

## Current UI Behavior

Management pages should show all rows that match the current search/filter settings. The table area reserves space for at least 20 rows, and grows taller if the filtered result has more than 20 records. Do not reintroduce hard limits such as `Take(5)` for Workers, Projects, Tasks, Attendance, Materials, or Issue Reports.

Add/edit dialogs should use dropdowns for existing related data instead of free typing where practical:

- Projects: choose the manager from existing workers.
- Tasks: choose the project and assigned worker from existing records.
- Attendance: choose the worker and project from existing records; the worker role and worker code should come from the selected worker.
- Issue Reports: choose the project and reporter from existing records.
- Materials: choose the category from existing material categories.

Login should stay as normal manual email/password entry for now. Do not add saved password suggestions unless explicitly requested again. If adding faster login later, prefer demo account buttons/dropdown over storing typed passwords.

## Project Report

`PROJECT_REPORT.md` has been created at the solution root. It summarizes the project overview, features, technology, OOP design, SQLite database integration, security awareness, testing/build verification, run instructions, and conclusion for submission or presentation.

## Latest Verification

Recent verification builds succeeded using separate output folders to avoid Windows file locks from a running app:

```powershell
dotnet build SmartConstructionSiteManagement\SmartConstructionSiteManagement.csproj --no-restore -p:UseAppHost=false -o .build-verify-20-rows
dotnet build SmartConstructionSiteManagement\SmartConstructionSiteManagement.csproj --no-restore -p:UseAppHost=false -o .build-verify-picker-dialogs
```

## Next Recommended Step

Run the app and manually check Add/Edit/Delete, search, and filters on Workers, Projects, Tasks, Attendance, Materials, and Issue Reports. If an old local SQLite database already exists, the app may keep old saved records until the database is reset.
