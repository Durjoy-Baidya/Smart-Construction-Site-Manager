# Smart Construction Site Manager

Smart Construction Site Manager is a C# WinForms desktop application for managing construction site operations. It helps organize workers, projects, tasks, attendance, materials, and issue reports in one centralized system.

Project website: https://durjoy-baidya.github.io/Smart-Construction-Site-Manager/

## Project Overview

This project was developed as a desktop application for managing construction site operations. The goal is to create a realistic but simple construction site management system with a user-friendly GUI, database integration, object-oriented design, and basic security awareness.

The current version is functionally complete for the final project submission.

## Technologies Used

- C#
- Windows Forms
- .NET `net10.0-windows`
- SQLite
- Microsoft.Data.Sqlite
- SHA-256 password hashing
- Git and GitHub

## Main Features

- Login system with user roles
- Role-based access control
- Dashboard overview
- Worker management
- Project management
- Task management
- Attendance tracking
- Material tracking
- Issue reporting
- Search and filter options
- Add, edit, delete, and view actions
- Realistic seed data

## Role-Based Access

| Role | Access |
| --- | --- |
| Admin | Can access all pages and add, edit, delete, and view records. |
| Site Manager | Can access all pages, manage daily site data, and edit workers, but cannot add or delete workers. |
| Worker | Can access only Dashboard, Tasks, and Issue Reports. Tasks are view-only; issue reports can be viewed and created, but not deleted. |

## Default Login Accounts

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@site.com` | `admin123` |
| Site Manager | `manager@site.com` | `manager123` |
| Worker | `worker@site.com` | `worker123` |

## Data Included

The application includes starter seed data:

- 3 users
- 20 workers
- 20 projects
- 20 tasks
- 20 attendance records
- 20 materials
- 20 issue reports

## Database

The application uses SQLite for local database storage. Data is loaded and saved through an `IAppDataStore` interface and a `SqliteAppDataStore` implementation.

The database stores:

- Users
- Workers
- Projects
- Tasks
- Attendance records
- Materials
- Issue reports

## Security Awareness

- Login is required before accessing the dashboard.
- Passwords are stored as SHA-256 hashes, not plain text.
- User roles are included: Admin, Site Manager, and Worker.
- Each role sees only the pages/actions allowed for that account.
- Password auto-suggestion/storage was intentionally not added.

## Object-Oriented Design

The project uses model and service classes such as:

- `UserAccount`
- `WorkerRecord`
- `ProjectRecord`
- `TaskRecord`
- `AttendanceRecord`
- `MaterialRecord`
- `IssueReportRecord`
- `AuthenticationService`
- `SqliteAppDataStore`

The dashboard is split into separate partial classes for each page to keep the code organized.

## How To Run

Open a terminal in the solution folder:

```powershell
cd "G:\Smart Construction Site Manager(App)"
```

Run the app:

```powershell
dotnet run --project SmartConstructionSiteManagement\SmartConstructionSiteManagement.csproj
```

## Build Verification

The project was verified using:

```powershell
dotnet build SmartConstructionSiteManagement\SmartConstructionSiteManagement.csproj --no-restore -p:UseAppHost=false -o .build-verify-final-report
```

The build completed with 0 warnings and 0 errors.

## Author

Durjoy Baidya
