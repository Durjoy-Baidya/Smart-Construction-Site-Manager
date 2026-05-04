# Smart Construction Site Manager - Project Report

## Project Overview

Smart Construction Site Manager is a C# Windows Forms desktop application for managing basic construction site operations. The system is designed as a realistic university project, with a simple user interface, database integration, role-based login, and management pages for common site activities.

The main goal of the application is to help a site manager or admin track workers, projects, tasks, attendance, materials, and issue reports from one dashboard.

## Technology Used

- Programming language: C#
- Application type: Windows Forms desktop application
- Framework: .NET `net10.0-windows`
- Database: SQLite
- SQLite package: `Microsoft.Data.Sqlite`
- IDE: Visual Studio / VS Code

## Main Features

### Login System

The application has a login screen with three default user roles:

- Admin
- Site Manager
- Worker

Default demo accounts:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@site.com` | `admin123` |
| Site Manager | `manager@site.com` | `manager123` |
| Worker | `worker@site.com` | `worker123` |

The login system checks the entered email and password before opening the dashboard.

### Dashboard

The dashboard gives a quick overview of the construction site data, including workers, projects, tasks, materials, and recent activity.

### Worker Management

The Workers page allows the user to:

- View all workers
- Search workers
- Filter by role and status
- Add workers
- Edit worker details
- Delete workers

### Project Management

The Projects page allows the user to:

- View all projects
- Search projects
- Filter by project status
- Add projects
- Edit projects
- Delete projects

Project managers can be selected from existing workers using a dropdown.

### Task Management

The Tasks page allows the user to:

- View all tasks
- Search tasks
- Filter by status and priority
- Add tasks
- Edit tasks
- Delete tasks

When adding or editing a task, the project and assigned worker can be selected from existing records.

### Attendance Management

The Attendance page allows the user to:

- View attendance records
- Search attendance
- Filter by date, project, and status
- Mark attendance
- View attendance details

When marking attendance, the worker and project are selected from existing records. The worker role and worker code are taken from the selected worker.

### Materials Management

The Materials page allows the user to:

- View material records
- Search materials
- Filter by category and stock status
- Add materials
- Edit materials
- Delete materials

Material categories are selected from existing material categories.

### Issue Reporting

The Issue Reports page allows the user to:

- View site issue reports
- Search issue reports
- Filter by status, priority, and project
- Report a new issue
- View issue details
- Delete issue reports

When reporting a new issue, the project and reporter can be selected from existing records.

## Seed Data

The application includes realistic starter data through `SeedData.cs`.

The default data includes:

- 3 users
- 20 workers
- 20 projects
- 20 tasks
- 20 attendance records
- 20 materials
- 20 issue reports

Seed data is only used when the application has no existing saved data. It does not overwrite an existing SQLite database.

## Database Integration

The project uses SQLite for data storage. This satisfies the database integration requirement while keeping the system realistic for a student desktop application.

The storage design uses an interface:

- `IAppDataStore`

The SQLite implementation is:

- `SqliteAppDataStore`

This means the user interface does not need to know the details of how data is saved. The app can load and save data through the storage interface.

The main stored data includes:

- Users
- Workers
- Projects
- Tasks
- Attendance records
- Materials
- Issue reports

## Object-Oriented Design

The project uses object-oriented programming through record/model classes and service classes.

Important classes include:

- `UserAccount`
- `WorkerRecord`
- `ProjectRecord`
- `TaskRecord`
- `AttendanceRecord`
- `MaterialRecord`
- `IssueReportRecord`
- `AuthenticationService`
- `SqliteAppDataStore`
- `DashboardForm`
- `LoginForm`

The dashboard is split into multiple partial classes so each page can be managed separately:

- `DashboardForm.WorkersPage.cs`
- `DashboardForm.ProjectsPage.cs`
- `DashboardForm.TasksPage.cs`
- `DashboardForm.AttendancePage.cs`
- `DashboardForm.MaterialsPage.cs`
- `DashboardForm.IssueReportsPage.cs`

This keeps the project easier to read and maintain.

## Security Awareness

The project includes basic security awareness:

- Passwords are not stored as plain text.
- Passwords are hashed using SHA-256 before being saved or checked.
- Login is required before accessing the dashboard.
- Different roles exist: Admin, Site Manager, and Worker.
- Password suggestion/storage was intentionally not added, because storing typed passwords would be insecure.

For a university project, SHA-256 hashing is better than plain text. In a real production system, a stronger password hashing method with salt, such as BCrypt or PBKDF2, would be recommended.

## User Interface

The app uses a Windows Forms graphical interface with:

- Login screen
- Sidebar navigation
- Dashboard page
- Management tables
- Search bars
- Filter dropdowns
- Add/edit dialogs
- Status badges
- Action buttons

Management pages show all rows that match the current filters. The table area is designed to support at least 20 rows and grows when more filtered records exist.

## Testing And Verification

The app was manually tested by checking:

- Login using default accounts
- Page navigation
- Search behavior
- Filter behavior
- Add, edit, delete, and view actions across management pages
- Dropdown selection for related data
- SQLite data persistence after saving

Build verification was also completed using:

```powershell
dotnet build SmartConstructionSiteManagement\SmartConstructionSiteManagement.csproj --no-restore -p:UseAppHost=false -o .build-verify-20-rows
dotnet build SmartConstructionSiteManagement\SmartConstructionSiteManagement.csproj --no-restore -p:UseAppHost=false -o .build-verify-picker-dialogs
```

Both verification builds completed successfully with 0 warnings and 0 errors.

## How To Run The Project

Open a terminal in the solution folder:

```powershell
cd "G:\Smart Construction Site Manager(App)"
```

Run the application:

```powershell
dotnet run --project SmartConstructionSiteManagement\SmartConstructionSiteManagement.csproj
```

If old saved data appears, reset the local SQLite database:

```powershell
Remove-Item -Force "$env:LOCALAPPDATA\SmartConstructionSiteManagement\smart-construction.db"
```

Then run the app again.

## Conclusion

Smart Construction Site Manager meets the main requirements of the project by providing a user-friendly desktop GUI, object-oriented design, SQLite database integration, login security awareness, realistic seed data, and build/manual testing evidence.

The application remains simple enough for a university project while still covering realistic construction site management workflows.
