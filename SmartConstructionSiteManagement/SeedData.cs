namespace SmartConstructionSiteManagement;

public static class SeedData
{
    public static ApplicationData CreateDefaultData()
    {
        return new ApplicationData
        {
            Users = CreateDefaultUsers(),
            Workers = CreateDefaultWorkers(),
            Projects = CreateDefaultProjects(),
            Tasks = CreateDefaultTasks(),
            AttendanceRecords = CreateDefaultAttendanceRecords(),
            Materials = CreateDefaultMaterials(),
            IssueReports = CreateDefaultIssueReports()
        };
    }

    public static bool HasAnyData(ApplicationData data)
    {
        return data.Users.Count > 0 || HasManagementData(data);
    }

    public static bool HasManagementData(ApplicationData data)
    {
        return data.Workers.Count > 0 ||
            data.Projects.Count > 0 ||
            data.Tasks.Count > 0 ||
            data.AttendanceRecords.Count > 0 ||
            data.Materials.Count > 0 ||
            data.IssueReports.Count > 0;
    }

    public static List<UserAccount> CreateDefaultUsers()
    {
        return
        [
            new UserAccount
            {
                Id = 1,
                Name = "Admin User",
                Email = "admin@site.com",
                PasswordHash = PasswordHasher.Hash("admin123"),
                Role = UserRole.Admin,
                ContactNumber = "+1 555-0001"
            },
            new UserAccount
            {
                Id = 2,
                Name = "John Anderson",
                Email = "manager@site.com",
                PasswordHash = PasswordHasher.Hash("manager123"),
                Role = UserRole.SiteManager,
                ContactNumber = "+1 555-0002"
            },
            new UserAccount
            {
                Id = 3,
                Name = "Emily Davis",
                Email = "worker@site.com",
                PasswordHash = PasswordHasher.Hash("worker123"),
                Role = UserRole.Worker,
                ContactNumber = "+1 555-0003"
            }
        ];
    }

    public static List<WorkerRecord> CreateDefaultWorkers()
    {
        return
        [
            new WorkerRecord { Id = 1, WorkerCode = "W001", Name = "Michael Thompson", Role = "Supervisor", Phone = "+1 555-0101", Email = "michael.t@example.com", HireDate = new DateTime(2025, 10, 14), Status = "Active" },
            new WorkerRecord { Id = 2, WorkerCode = "W002", Name = "Sarah Johnson", Role = "Electrician", Phone = "+1 555-0102", Email = "sarah.j@example.com", HireDate = new DateTime(2025, 11, 3), Status = "Active" },
            new WorkerRecord { Id = 3, WorkerCode = "W003", Name = "David Martinez", Role = "Carpenter", Phone = "+1 555-0103", Email = "david.m@example.com", HireDate = new DateTime(2025, 11, 18), Status = "On Leave" },
            new WorkerRecord { Id = 4, WorkerCode = "W004", Name = "James Wilson", Role = "Plumber", Phone = "+1 555-0104", Email = "james.w@example.com", HireDate = new DateTime(2025, 12, 1), Status = "Active" },
            new WorkerRecord { Id = 5, WorkerCode = "W005", Name = "Emily Davis", Role = "Mason", Phone = "+1 555-0105", Email = "emily.d@example.com", HireDate = new DateTime(2025, 12, 12), Status = "Active" },
            new WorkerRecord { Id = 6, WorkerCode = "W006", Name = "Robert Brown", Role = "Safety Officer", Phone = "+1 555-0106", Email = "robert.b@example.com", HireDate = new DateTime(2026, 1, 7), Status = "Active" },
            new WorkerRecord { Id = 7, WorkerCode = "W007", Name = "Linda Walker", Role = "Mason", Phone = "+1 555-0107", Email = "linda.w@example.com", HireDate = new DateTime(2026, 1, 22), Status = "Active" },
            new WorkerRecord { Id = 8, WorkerCode = "W008", Name = "Kevin Lee", Role = "Electrician", Phone = "+1 555-0108", Email = "kevin.l@example.com", HireDate = new DateTime(2026, 2, 5), Status = "Inactive" },
            new WorkerRecord { Id = 9, WorkerCode = "W009", Name = "Maria Garcia", Role = "Carpenter", Phone = "+1 555-0109", Email = "maria.g@example.com", HireDate = new DateTime(2026, 2, 17), Status = "Active" },
            new WorkerRecord { Id = 10, WorkerCode = "W010", Name = "Ahmed Khan", Role = "Mason", Phone = "+1 555-0110", Email = "ahmed.k@example.com", HireDate = new DateTime(2026, 3, 4), Status = "Active" },
            new WorkerRecord { Id = 11, WorkerCode = "W011", Name = "Patricia Moore", Role = "Supervisor", Phone = "+1 555-0111", Email = "patricia.m@example.com", HireDate = new DateTime(2026, 3, 12), Status = "Active" },
            new WorkerRecord { Id = 12, WorkerCode = "W012", Name = "Daniel Clark", Role = "Plumber", Phone = "+1 555-0112", Email = "daniel.c@example.com", HireDate = new DateTime(2026, 3, 20), Status = "Active" },
            new WorkerRecord { Id = 13, WorkerCode = "W013", Name = "Nancy Hall", Role = "Electrician", Phone = "+1 555-0113", Email = "nancy.h@example.com", HireDate = new DateTime(2026, 3, 28), Status = "On Leave" },
            new WorkerRecord { Id = 14, WorkerCode = "W014", Name = "Thomas Young", Role = "Carpenter", Phone = "+1 555-0114", Email = "thomas.y@example.com", HireDate = new DateTime(2026, 4, 2), Status = "Active" },
            new WorkerRecord { Id = 15, WorkerCode = "W015", Name = "Sophia Allen", Role = "Safety Officer", Phone = "+1 555-0115", Email = "sophia.a@example.com", HireDate = new DateTime(2026, 4, 8), Status = "Active" },
            new WorkerRecord { Id = 16, WorkerCode = "W016", Name = "George Wright", Role = "Mason", Phone = "+1 555-0116", Email = "george.w@example.com", HireDate = new DateTime(2026, 4, 15), Status = "Inactive" },
            new WorkerRecord { Id = 17, WorkerCode = "W017", Name = "Olivia King", Role = "Electrician", Phone = "+1 555-0117", Email = "olivia.k@example.com", HireDate = new DateTime(2026, 4, 21), Status = "Active" },
            new WorkerRecord { Id = 18, WorkerCode = "W018", Name = "Brian Scott", Role = "Plumber", Phone = "+1 555-0118", Email = "brian.s@example.com", HireDate = new DateTime(2026, 4, 26), Status = "Active" },
            new WorkerRecord { Id = 19, WorkerCode = "W019", Name = "Grace Turner", Role = "Carpenter", Phone = "+1 555-0119", Email = "grace.t@example.com", HireDate = new DateTime(2026, 5, 1), Status = "On Leave" },
            new WorkerRecord { Id = 20, WorkerCode = "W020", Name = "Henry Adams", Role = "Mason", Phone = "+1 555-0120", Email = "henry.a@example.com", HireDate = new DateTime(2026, 5, 6), Status = "Active" }
        ];
    }

    public static List<ProjectRecord> CreateDefaultProjects()
    {
        return
        [
            new ProjectRecord { Id = 1, ProjectCode = "P001", Name = "Project Alpha", Location = "Downtown Block A", StartDate = new DateTime(2026, 1, 12), EndDate = new DateTime(2026, 9, 30), Manager = "John Anderson", Status = "In Progress", Progress = 68 },
            new ProjectRecord { Id = 2, ProjectCode = "P002", Name = "Office Tower", Location = "City Center", StartDate = new DateTime(2025, 11, 20), EndDate = new DateTime(2026, 8, 15), Manager = "Michael Thompson", Status = "Delayed", Progress = 74 },
            new ProjectRecord { Id = 3, ProjectCode = "P003", Name = "Bridge Repair", Location = "North Road", StartDate = new DateTime(2026, 2, 5), EndDate = new DateTime(2026, 7, 20), Manager = "John Anderson", Status = "In Progress", Progress = 52 },
            new ProjectRecord { Id = 4, ProjectCode = "P004", Name = "Warehouse Extension", Location = "Industrial Zone", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 4, 24), Manager = "James Wilson", Status = "Completed", Progress = 100 },
            new ProjectRecord { Id = 5, ProjectCode = "P005", Name = "Residential Complex", Location = "West Avenue", StartDate = new DateTime(2026, 4, 8), EndDate = new DateTime(2026, 12, 18), Manager = "John Anderson", Status = "Planning", Progress = 12 },
            new ProjectRecord { Id = 6, ProjectCode = "P006", Name = "Hospital Renovation", Location = "East District", StartDate = new DateTime(2026, 3, 2), EndDate = new DateTime(2026, 11, 28), Manager = "Sarah Johnson", Status = "In Progress", Progress = 36 },
            new ProjectRecord { Id = 7, ProjectCode = "P007", Name = "School Building", Location = "Greenfield Road", StartDate = new DateTime(2026, 5, 11), EndDate = new DateTime(2027, 2, 10), Manager = "Robert Brown", Status = "Planning", Progress = 8 },
            new ProjectRecord { Id = 8, ProjectCode = "P008", Name = "Mall Expansion", Location = "South Avenue", StartDate = new DateTime(2025, 12, 6), EndDate = new DateTime(2026, 10, 5), Manager = "Maria Garcia", Status = "Delayed", Progress = 48 },
            new ProjectRecord { Id = 9, ProjectCode = "P009", Name = "Road Drainage Upgrade", Location = "Riverside Lane", StartDate = new DateTime(2026, 1, 25), EndDate = new DateTime(2026, 6, 18), Manager = "David Martinez", Status = "Completed", Progress = 100 },
            new ProjectRecord { Id = 10, ProjectCode = "P010", Name = "Parking Structure", Location = "Central Station", StartDate = new DateTime(2026, 4, 20), EndDate = new DateTime(2026, 12, 22), Manager = "Kevin Lee", Status = "In Progress", Progress = 21 },
            new ProjectRecord { Id = 11, ProjectCode = "P011", Name = "Community Center", Location = "Lakeview", StartDate = new DateTime(2026, 2, 14), EndDate = new DateTime(2026, 9, 12), Manager = "Patricia Moore", Status = "In Progress", Progress = 44 },
            new ProjectRecord { Id = 12, ProjectCode = "P012", Name = "Fire Station Upgrade", Location = "Northwest Yard", StartDate = new DateTime(2026, 3, 18), EndDate = new DateTime(2026, 8, 30), Manager = "Sophia Allen", Status = "Planning", Progress = 15 },
            new ProjectRecord { Id = 13, ProjectCode = "P013", Name = "Water Treatment Unit", Location = "River Plant", StartDate = new DateTime(2025, 10, 10), EndDate = new DateTime(2026, 7, 14), Manager = "Daniel Clark", Status = "Delayed", Progress = 62 },
            new ProjectRecord { Id = 14, ProjectCode = "P014", Name = "Library Annex", Location = "Old Town", StartDate = new DateTime(2026, 1, 30), EndDate = new DateTime(2026, 5, 30), Manager = "Thomas Young", Status = "Completed", Progress = 100 },
            new ProjectRecord { Id = 15, ProjectCode = "P015", Name = "Sports Hall", Location = "Campus Road", StartDate = new DateTime(2026, 5, 4), EndDate = new DateTime(2027, 1, 19), Manager = "Olivia King", Status = "Planning", Progress = 5 },
            new ProjectRecord { Id = 16, ProjectCode = "P016", Name = "Transit Shelter Package", Location = "Citywide", StartDate = new DateTime(2026, 2, 21), EndDate = new DateTime(2026, 6, 30), Manager = "Brian Scott", Status = "In Progress", Progress = 58 },
            new ProjectRecord { Id = 17, ProjectCode = "P017", Name = "Hotel Fit Out", Location = "Market Square", StartDate = new DateTime(2025, 12, 16), EndDate = new DateTime(2026, 9, 4), Manager = "Grace Turner", Status = "Delayed", Progress = 53 },
            new ProjectRecord { Id = 18, ProjectCode = "P018", Name = "Warehouse Office Block", Location = "Logistics Park", StartDate = new DateTime(2026, 3, 25), EndDate = new DateTime(2026, 10, 16), Manager = "Henry Adams", Status = "In Progress", Progress = 31 },
            new ProjectRecord { Id = 19, ProjectCode = "P019", Name = "Pedestrian Overpass", Location = "Station Road", StartDate = new DateTime(2026, 4, 9), EndDate = new DateTime(2026, 11, 2), Manager = "Nancy Hall", Status = "Planning", Progress = 9 },
            new ProjectRecord { Id = 20, ProjectCode = "P020", Name = "Clinic Extension", Location = "Hill Street", StartDate = new DateTime(2025, 8, 15), EndDate = new DateTime(2026, 3, 28), Manager = "George Wright", Status = "Completed", Progress = 100 }
        ];
    }

    public static List<TaskRecord> CreateDefaultTasks()
    {
        return
        [
            new TaskRecord { Id = 1, TaskCode = "T001", Title = "Install site safety barriers", ProjectName = "Project Alpha", AssignedTo = "Michael Thompson", Priority = "High", DueDate = new DateTime(2026, 5, 6), Status = "In Progress" },
            new TaskRecord { Id = 2, TaskCode = "T002", Title = "Check electrical wiring on level 2", ProjectName = "Office Tower", AssignedTo = "Sarah Johnson", Priority = "High", DueDate = new DateTime(2026, 5, 4), Status = "Pending" },
            new TaskRecord { Id = 3, TaskCode = "T003", Title = "Prepare concrete foundation area", ProjectName = "Bridge Repair", AssignedTo = "David Martinez", Priority = "Medium", DueDate = new DateTime(2026, 5, 12), Status = "In Progress" },
            new TaskRecord { Id = 4, TaskCode = "T004", Title = "Inspect plumbing layout", ProjectName = "Warehouse Extension", AssignedTo = "James Wilson", Priority = "Medium", DueDate = new DateTime(2026, 4, 22), Status = "Completed" },
            new TaskRecord { Id = 5, TaskCode = "T005", Title = "Order bricks for boundary wall", ProjectName = "Residential Complex", AssignedTo = "Emily Davis", Priority = "Low", DueDate = new DateTime(2026, 4, 28), Status = "Overdue" },
            new TaskRecord { Id = 6, TaskCode = "T006", Title = "Review scaffold safety checklist", ProjectName = "Office Tower", AssignedTo = "Robert Brown", Priority = "High", DueDate = new DateTime(2026, 5, 1), Status = "In Progress" },
            new TaskRecord { Id = 7, TaskCode = "T007", Title = "Lay blockwork for storage room", ProjectName = "Project Alpha", AssignedTo = "Linda Walker", Priority = "Medium", DueDate = new DateTime(2026, 5, 9), Status = "Pending" },
            new TaskRecord { Id = 8, TaskCode = "T008", Title = "Repair temporary lighting", ProjectName = "Bridge Repair", AssignedTo = "Kevin Lee", Priority = "Medium", DueDate = new DateTime(2026, 4, 25), Status = "Overdue" },
            new TaskRecord { Id = 9, TaskCode = "T009", Title = "Complete roof frame inspection", ProjectName = "Warehouse Extension", AssignedTo = "Maria Garcia", Priority = "Low", DueDate = new DateTime(2026, 4, 20), Status = "Completed" },
            new TaskRecord { Id = 10, TaskCode = "T010", Title = "Prepare material request form", ProjectName = "Residential Complex", AssignedTo = "Ahmed Khan", Priority = "Low", DueDate = new DateTime(2026, 5, 7), Status = "Pending" },
            new TaskRecord { Id = 11, TaskCode = "T011", Title = "Check steel column alignment", ProjectName = "Hospital Renovation", AssignedTo = "Patricia Moore", Priority = "High", DueDate = new DateTime(2026, 5, 13), Status = "In Progress" },
            new TaskRecord { Id = 12, TaskCode = "T012", Title = "Install temporary water line", ProjectName = "School Building", AssignedTo = "Daniel Clark", Priority = "Medium", DueDate = new DateTime(2026, 5, 15), Status = "Pending" },
            new TaskRecord { Id = 13, TaskCode = "T013", Title = "Test emergency lighting", ProjectName = "Mall Expansion", AssignedTo = "Nancy Hall", Priority = "High", DueDate = new DateTime(2026, 5, 3), Status = "Overdue" },
            new TaskRecord { Id = 14, TaskCode = "T014", Title = "Finish drainage trench backfill", ProjectName = "Road Drainage Upgrade", AssignedTo = "Thomas Young", Priority = "Medium", DueDate = new DateTime(2026, 4, 29), Status = "Completed" },
            new TaskRecord { Id = 15, TaskCode = "T015", Title = "Paint level one stairwell", ProjectName = "Parking Structure", AssignedTo = "Sophia Allen", Priority = "Low", DueDate = new DateTime(2026, 5, 16), Status = "Pending" },
            new TaskRecord { Id = 16, TaskCode = "T016", Title = "Inspect concrete pour area", ProjectName = "Community Center", AssignedTo = "George Wright", Priority = "High", DueDate = new DateTime(2026, 5, 10), Status = "In Progress" },
            new TaskRecord { Id = 17, TaskCode = "T017", Title = "Install door frames", ProjectName = "Fire Station Upgrade", AssignedTo = "Olivia King", Priority = "Medium", DueDate = new DateTime(2026, 5, 18), Status = "Pending" },
            new TaskRecord { Id = 18, TaskCode = "T018", Title = "Review tank foundation drawings", ProjectName = "Water Treatment Unit", AssignedTo = "Brian Scott", Priority = "High", DueDate = new DateTime(2026, 5, 2), Status = "Overdue" },
            new TaskRecord { Id = 19, TaskCode = "T019", Title = "Complete final cleaning", ProjectName = "Library Annex", AssignedTo = "Grace Turner", Priority = "Low", DueDate = new DateTime(2026, 4, 27), Status = "Completed" },
            new TaskRecord { Id = 20, TaskCode = "T020", Title = "Prepare roof truss delivery area", ProjectName = "Sports Hall", AssignedTo = "Henry Adams", Priority = "Medium", DueDate = new DateTime(2026, 5, 20), Status = "Pending" }
        ];
    }

    public static List<AttendanceRecord> CreateDefaultAttendanceRecords()
    {
        DateTime attendanceDate = new(2026, 4, 30);

        return
        [
            new AttendanceRecord { Id = 1, WorkerCode = "W001", WorkerName = "Michael Thompson", Role = "Supervisor", ProjectName = "Project Alpha", Date = attendanceDate, CheckIn = "08:00 AM", CheckOut = "05:00 PM", Status = "Present" },
            new AttendanceRecord { Id = 2, WorkerCode = "W002", WorkerName = "Sarah Johnson", Role = "Electrician", ProjectName = "Office Tower", Date = attendanceDate, CheckIn = "08:05 AM", CheckOut = "04:45 PM", Status = "Present" },
            new AttendanceRecord { Id = 3, WorkerCode = "W003", WorkerName = "David Martinez", Role = "Carpenter", ProjectName = "Bridge Repair", Date = attendanceDate, CheckIn = "-", CheckOut = "-", Status = "Absent" },
            new AttendanceRecord { Id = 4, WorkerCode = "W004", WorkerName = "James Wilson", Role = "Plumber", ProjectName = "Warehouse Extension", Date = attendanceDate, CheckIn = "08:20 AM", CheckOut = "05:00 PM", Status = "Present" },
            new AttendanceRecord { Id = 5, WorkerCode = "W005", WorkerName = "Emily Davis", Role = "Mason", ProjectName = "Residential Complex", Date = attendanceDate, CheckIn = "08:10 AM", CheckOut = "04:55 PM", Status = "Present" },
            new AttendanceRecord { Id = 6, WorkerCode = "W006", WorkerName = "Robert Brown", Role = "Safety Officer", ProjectName = "Office Tower", Date = attendanceDate, CheckIn = "07:50 AM", CheckOut = "05:10 PM", Status = "Present" },
            new AttendanceRecord { Id = 7, WorkerCode = "W007", WorkerName = "Linda Walker", Role = "Mason", ProjectName = "Project Alpha", Date = attendanceDate, CheckIn = "08:15 AM", CheckOut = "05:05 PM", Status = "Present" },
            new AttendanceRecord { Id = 8, WorkerCode = "W008", WorkerName = "Kevin Lee", Role = "Electrician", ProjectName = "Bridge Repair", Date = attendanceDate, CheckIn = "-", CheckOut = "-", Status = "Absent" },
            new AttendanceRecord { Id = 9, WorkerCode = "W009", WorkerName = "Maria Garcia", Role = "Carpenter", ProjectName = "Warehouse Extension", Date = attendanceDate, CheckIn = "08:00 AM", CheckOut = "04:50 PM", Status = "Present" },
            new AttendanceRecord { Id = 10, WorkerCode = "W010", WorkerName = "Ahmed Khan", Role = "Mason", ProjectName = "Residential Complex", Date = attendanceDate, CheckIn = "08:25 AM", CheckOut = "05:00 PM", Status = "Present" },
            new AttendanceRecord { Id = 11, WorkerCode = "W011", WorkerName = "Patricia Moore", Role = "Supervisor", ProjectName = "Hospital Renovation", Date = attendanceDate, CheckIn = "07:55 AM", CheckOut = "05:05 PM", Status = "Present" },
            new AttendanceRecord { Id = 12, WorkerCode = "W012", WorkerName = "Daniel Clark", Role = "Plumber", ProjectName = "School Building", Date = attendanceDate, CheckIn = "08:12 AM", CheckOut = "04:45 PM", Status = "Present" },
            new AttendanceRecord { Id = 13, WorkerCode = "W013", WorkerName = "Nancy Hall", Role = "Electrician", ProjectName = "Mall Expansion", Date = attendanceDate, CheckIn = "-", CheckOut = "-", Status = "Absent" },
            new AttendanceRecord { Id = 14, WorkerCode = "W014", WorkerName = "Thomas Young", Role = "Carpenter", ProjectName = "Road Drainage Upgrade", Date = attendanceDate, CheckIn = "08:04 AM", CheckOut = "05:00 PM", Status = "Present" },
            new AttendanceRecord { Id = 15, WorkerCode = "W015", WorkerName = "Sophia Allen", Role = "Safety Officer", ProjectName = "Parking Structure", Date = attendanceDate, CheckIn = "07:45 AM", CheckOut = "05:20 PM", Status = "Present" },
            new AttendanceRecord { Id = 16, WorkerCode = "W016", WorkerName = "George Wright", Role = "Mason", ProjectName = "Community Center", Date = attendanceDate, CheckIn = "-", CheckOut = "-", Status = "Absent" },
            new AttendanceRecord { Id = 17, WorkerCode = "W017", WorkerName = "Olivia King", Role = "Electrician", ProjectName = "Fire Station Upgrade", Date = attendanceDate, CheckIn = "08:08 AM", CheckOut = "04:55 PM", Status = "Present" },
            new AttendanceRecord { Id = 18, WorkerCode = "W018", WorkerName = "Brian Scott", Role = "Plumber", ProjectName = "Water Treatment Unit", Date = attendanceDate, CheckIn = "08:18 AM", CheckOut = "05:10 PM", Status = "Present" },
            new AttendanceRecord { Id = 19, WorkerCode = "W019", WorkerName = "Grace Turner", Role = "Carpenter", ProjectName = "Library Annex", Date = attendanceDate, CheckIn = "-", CheckOut = "-", Status = "Absent" },
            new AttendanceRecord { Id = 20, WorkerCode = "W020", WorkerName = "Henry Adams", Role = "Mason", ProjectName = "Sports Hall", Date = attendanceDate, CheckIn = "08:03 AM", CheckOut = "04:58 PM", Status = "Present" }
        ];
    }

    public static List<MaterialRecord> CreateDefaultMaterials()
    {
        return
        [
            new MaterialRecord { Id = 1, Name = "Cement Bags (50kg)", Category = "Cement", Unit = "Bag", Quantity = 320, UnitPrice = 8.50m, Status = "In Stock" },
            new MaterialRecord { Id = 2, Name = "Rapid Set Cement", Category = "Cement", Unit = "Bag", Quantity = 42, UnitPrice = 12.75m, Status = "Low Stock" },
            new MaterialRecord { Id = 3, Name = "Steel Rebar (12mm)", Category = "Steel", Unit = "Piece", Quantity = 450, UnitPrice = 6.20m, Status = "In Stock" },
            new MaterialRecord { Id = 4, Name = "Steel Mesh Sheets", Category = "Steel", Unit = "Sheet", Quantity = 75, UnitPrice = 18.40m, Status = "In Stock" },
            new MaterialRecord { Id = 5, Name = "Bricks (Standard)", Category = "Bricks", Unit = "Piece", Quantity = 2500, UnitPrice = 0.45m, Status = "In Stock" },
            new MaterialRecord { Id = 6, Name = "Facing Bricks", Category = "Bricks", Unit = "Piece", Quantity = 390, UnitPrice = 0.62m, Status = "Low Stock" },
            new MaterialRecord { Id = 7, Name = "Sand (Fine)", Category = "Aggregates", Unit = "m3", Quantity = 35, UnitPrice = 28.00m, Status = "In Stock" },
            new MaterialRecord { Id = 8, Name = "Crushed Stone (20mm)", Category = "Aggregates", Unit = "m3", Quantity = 40, UnitPrice = 32.50m, Status = "In Stock" },
            new MaterialRecord { Id = 9, Name = "Gravel Mix", Category = "Aggregates", Unit = "m3", Quantity = 18, UnitPrice = 30.25m, Status = "Low Stock" },
            new MaterialRecord { Id = 10, Name = "Mortar Mix", Category = "Cement", Unit = "Bag", Quantity = 0, UnitPrice = 9.80m, Status = "Out of Stock" },
            new MaterialRecord { Id = 11, Name = "Plywood Sheets", Category = "Timber", Unit = "Sheet", Quantity = 85, UnitPrice = 24.50m, Status = "In Stock" },
            new MaterialRecord { Id = 12, Name = "Timber Beams", Category = "Timber", Unit = "Piece", Quantity = 28, UnitPrice = 36.75m, Status = "Low Stock" },
            new MaterialRecord { Id = 13, Name = "PVC Pipes", Category = "Plumbing", Unit = "Piece", Quantity = 160, UnitPrice = 4.80m, Status = "In Stock" },
            new MaterialRecord { Id = 14, Name = "Copper Fittings", Category = "Plumbing", Unit = "Box", Quantity = 12, UnitPrice = 44.00m, Status = "Low Stock" },
            new MaterialRecord { Id = 15, Name = "Electrical Cable Rolls", Category = "Electrical", Unit = "Roll", Quantity = 22, UnitPrice = 68.50m, Status = "In Stock" },
            new MaterialRecord { Id = 16, Name = "Switch Boxes", Category = "Electrical", Unit = "Box", Quantity = 0, UnitPrice = 18.25m, Status = "Out of Stock" },
            new MaterialRecord { Id = 17, Name = "Wall Tiles", Category = "Finishing", Unit = "Box", Quantity = 64, UnitPrice = 21.90m, Status = "In Stock" },
            new MaterialRecord { Id = 18, Name = "Floor Tiles", Category = "Finishing", Unit = "Box", Quantity = 30, UnitPrice = 28.75m, Status = "Low Stock" },
            new MaterialRecord { Id = 19, Name = "Safety Helmets", Category = "Safety", Unit = "Piece", Quantity = 45, UnitPrice = 11.50m, Status = "In Stock" },
            new MaterialRecord { Id = 20, Name = "Reflective Vests", Category = "Safety", Unit = "Piece", Quantity = 0, UnitPrice = 8.25m, Status = "Out of Stock" }
        ];
    }

    public static List<IssueReportRecord> CreateDefaultIssueReports()
    {
        return
        [
            new IssueReportRecord { Id = 1, Title = "Cracked Concrete Wall", ProjectName = "Office Tower", ReportedBy = "Sarah Johnson", Priority = "High", Status = "Open", ReportedDate = new DateTime(2026, 4, 30), Description = "Visible cracks found on a concrete wall during inspection." },
            new IssueReportRecord { Id = 2, Title = "Material Delivery Delay", ProjectName = "Bridge Repair", ReportedBy = "David Martinez", Priority = "Medium", Status = "In Progress", ReportedDate = new DateTime(2026, 4, 29), Description = "Steel delivery is delayed and may affect task scheduling." },
            new IssueReportRecord { Id = 3, Title = "Temporary Wiring Needs Review", ProjectName = "Project Alpha", ReportedBy = "Michael Thompson", Priority = "High", Status = "Open", ReportedDate = new DateTime(2026, 4, 28), Description = "Temporary wiring should be reviewed before work continues near the storage area." },
            new IssueReportRecord { Id = 4, Title = "Safety Equipment Missing", ProjectName = "Residential Complex", ReportedBy = "Robert Brown", Priority = "Low", Status = "Resolved", ReportedDate = new DateTime(2026, 4, 25), Description = "Extra helmets and gloves were requested and supplied." },
            new IssueReportRecord { Id = 5, Title = "Plumbing Leak", ProjectName = "Warehouse Extension", ReportedBy = "James Wilson", Priority = "Medium", Status = "In Progress", ReportedDate = new DateTime(2026, 4, 24), Description = "Leak detected near a temporary plumbing line." },
            new IssueReportRecord { Id = 6, Title = "Scaffold Tag Expired", ProjectName = "Office Tower", ReportedBy = "Robert Brown", Priority = "High", Status = "Open", ReportedDate = new DateTime(2026, 4, 23), Description = "Scaffold inspection tag must be renewed before the next shift." },
            new IssueReportRecord { Id = 7, Title = "Low Brick Stock", ProjectName = "Residential Complex", ReportedBy = "Emily Davis", Priority = "Low", Status = "Resolved", ReportedDate = new DateTime(2026, 4, 22), Description = "Brick quantity was low, and a new order was submitted." },
            new IssueReportRecord { Id = 8, Title = "Loose Guardrail", ProjectName = "Parking Structure", ReportedBy = "Sophia Allen", Priority = "High", Status = "Open", ReportedDate = new DateTime(2026, 4, 21), Description = "Temporary guardrail near ramp opening needs to be secured." },
            new IssueReportRecord { Id = 9, Title = "Blocked Access Route", ProjectName = "Hospital Renovation", ReportedBy = "Patricia Moore", Priority = "Medium", Status = "In Progress", ReportedDate = new DateTime(2026, 4, 20), Description = "Stored materials are blocking the emergency access path." },
            new IssueReportRecord { Id = 10, Title = "Damaged Pipe Section", ProjectName = "School Building", ReportedBy = "Daniel Clark", Priority = "Medium", Status = "Open", ReportedDate = new DateTime(2026, 4, 19), Description = "A PVC pipe section was damaged during unloading." },
            new IssueReportRecord { Id = 11, Title = "Cable Tray Misalignment", ProjectName = "Mall Expansion", ReportedBy = "Nancy Hall", Priority = "Medium", Status = "In Progress", ReportedDate = new DateTime(2026, 4, 18), Description = "Cable tray alignment does not match the approved drawing." },
            new IssueReportRecord { Id = 12, Title = "Drain Cover Missing", ProjectName = "Road Drainage Upgrade", ReportedBy = "Thomas Young", Priority = "High", Status = "Resolved", ReportedDate = new DateTime(2026, 4, 17), Description = "Temporary drain cover was missing and has been replaced." },
            new IssueReportRecord { Id = 13, Title = "Unlabeled Chemical Container", ProjectName = "Community Center", ReportedBy = "George Wright", Priority = "High", Status = "Open", ReportedDate = new DateTime(2026, 4, 16), Description = "Container in storage area needs correct safety labeling." },
            new IssueReportRecord { Id = 14, Title = "Incorrect Door Frame Size", ProjectName = "Fire Station Upgrade", ReportedBy = "Olivia King", Priority = "Medium", Status = "Open", ReportedDate = new DateTime(2026, 4, 15), Description = "Delivered door frame does not match the opening measurement." },
            new IssueReportRecord { Id = 15, Title = "Standing Water Near Tank Base", ProjectName = "Water Treatment Unit", ReportedBy = "Brian Scott", Priority = "High", Status = "In Progress", ReportedDate = new DateTime(2026, 4, 14), Description = "Water has collected near the tank foundation after rainfall." },
            new IssueReportRecord { Id = 16, Title = "Paint Finish Defect", ProjectName = "Library Annex", ReportedBy = "Grace Turner", Priority = "Low", Status = "Resolved", ReportedDate = new DateTime(2026, 4, 13), Description = "Paint finish defect on corridor wall was corrected." },
            new IssueReportRecord { Id = 17, Title = "Roof Truss Delivery Delay", ProjectName = "Sports Hall", ReportedBy = "Henry Adams", Priority = "Medium", Status = "Open", ReportedDate = new DateTime(2026, 4, 12), Description = "Roof truss delivery is delayed by supplier scheduling." },
            new IssueReportRecord { Id = 18, Title = "Concrete Sample Failed", ProjectName = "Hotel Fit Out", ReportedBy = "Maria Garcia", Priority = "High", Status = "In Progress", ReportedDate = new DateTime(2026, 4, 11), Description = "Concrete sample result requires engineer review before next pour." },
            new IssueReportRecord { Id = 19, Title = "Low Lighting In Work Zone", ProjectName = "Warehouse Office Block", ReportedBy = "Ahmed Khan", Priority = "Low", Status = "Open", ReportedDate = new DateTime(2026, 4, 10), Description = "Temporary lighting is weak in the west work zone." },
            new IssueReportRecord { Id = 20, Title = "Overpass Bolt Check Required", ProjectName = "Pedestrian Overpass", ReportedBy = "Linda Walker", Priority = "Medium", Status = "Resolved", ReportedDate = new DateTime(2026, 4, 9), Description = "Bolt inspection checklist was completed and filed." }
        ];
    }
}
