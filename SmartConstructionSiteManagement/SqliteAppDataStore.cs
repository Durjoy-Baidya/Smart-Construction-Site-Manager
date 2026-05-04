using Microsoft.Data.Sqlite;
using System.Globalization;

namespace SmartConstructionSiteManagement;

public class SqliteAppDataStore : IAppDataStore
{
    private readonly string connectionString;

    public SqliteAppDataStore()
    {
        string dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartConstructionSiteManagement");

        Directory.CreateDirectory(dataDirectory);
        string databasePath = Path.Combine(dataDirectory, "smart-construction.db");
        connectionString = new SqliteConnectionStringBuilder { DataSource = databasePath }.ToString();

        InitializeDatabase();
    }

    public ApplicationData? Load()
    {
        using SqliteConnection connection = CreateOpenConnection();

        return new ApplicationData
        {
            Users = LoadUsers(connection),
            Workers = LoadWorkers(connection),
            Projects = LoadProjects(connection),
            Tasks = LoadTasks(connection),
            AttendanceRecords = LoadAttendance(connection),
            Materials = LoadMaterials(connection),
            IssueReports = LoadIssueReports(connection)
        };
    }

    public void Save(ApplicationData data)
    {
        using SqliteConnection connection = CreateOpenConnection();
        using SqliteTransaction transaction = connection.BeginTransaction();

        ClearTables(connection, transaction);
        SaveUsers(connection, transaction, data.Users);
        SaveWorkers(connection, transaction, data.Workers);
        SaveProjects(connection, transaction, data.Projects);
        SaveTasks(connection, transaction, data.Tasks);
        SaveAttendance(connection, transaction, data.AttendanceRecords);
        SaveMaterials(connection, transaction, data.Materials);
        SaveIssueReports(connection, transaction, data.IssueReports);

        transaction.Commit();
    }

    private SqliteConnection CreateOpenConnection()
    {
        SqliteConnection connection = new(connectionString);
        connection.Open();
        return connection;
    }

    private void InitializeDatabase()
    {
        using SqliteConnection connection = CreateOpenConnection();

        ExecuteNonQuery(connection, null, """
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Role TEXT NOT NULL,
                ContactNumber TEXT
            );

            CREATE TABLE IF NOT EXISTS Workers (
                Id INTEGER PRIMARY KEY,
                WorkerCode TEXT NOT NULL,
                Name TEXT NOT NULL,
                Role TEXT NOT NULL,
                Phone TEXT,
                Email TEXT,
                HireDate TEXT NOT NULL,
                Status TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Projects (
                Id INTEGER PRIMARY KEY,
                ProjectCode TEXT NOT NULL,
                Name TEXT NOT NULL,
                Location TEXT NOT NULL,
                StartDate TEXT NOT NULL,
                EndDate TEXT NOT NULL,
                Manager TEXT,
                Status TEXT NOT NULL,
                Progress INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Tasks (
                Id INTEGER PRIMARY KEY,
                TaskCode TEXT NOT NULL,
                Title TEXT NOT NULL,
                ProjectName TEXT NOT NULL,
                AssignedTo TEXT NOT NULL,
                Priority TEXT NOT NULL,
                DueDate TEXT NOT NULL,
                Status TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Attendance (
                Id INTEGER PRIMARY KEY,
                WorkerCode TEXT NOT NULL,
                WorkerName TEXT NOT NULL,
                Role TEXT NOT NULL,
                ProjectName TEXT NOT NULL,
                Date TEXT NOT NULL,
                CheckIn TEXT,
                CheckOut TEXT,
                Status TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Materials (
                Id INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Category TEXT NOT NULL,
                Unit TEXT NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitPrice TEXT NOT NULL,
                Status TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS IssueReports (
                Id INTEGER PRIMARY KEY,
                Title TEXT NOT NULL,
                ProjectName TEXT NOT NULL,
                ReportedBy TEXT NOT NULL,
                Priority TEXT NOT NULL,
                Status TEXT NOT NULL,
                ReportedDate TEXT NOT NULL,
                Description TEXT
            );
            """);
    }

    private static void ClearTables(SqliteConnection connection, SqliteTransaction transaction)
    {
        string[] tables = ["IssueReports", "Materials", "Attendance", "Tasks", "Projects", "Workers", "Users"];

        foreach (string table in tables)
        {
            ExecuteNonQuery(connection, transaction, $"DELETE FROM {table};");
        }
    }

    private static List<UserAccount> LoadUsers(SqliteConnection connection)
    {
        List<UserAccount> users = [];
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Email, PasswordHash, Role, ContactNumber FROM Users ORDER BY Id;";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(new UserAccount
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                Role = Enum.Parse<UserRole>(reader.GetString(4)),
                ContactNumber = GetString(reader, 5)
            });
        }

        return users;
    }

    private static List<WorkerRecord> LoadWorkers(SqliteConnection connection)
    {
        List<WorkerRecord> workers = [];
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, WorkerCode, Name, Role, Phone, Email, HireDate, Status FROM Workers ORDER BY Id;";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            workers.Add(new WorkerRecord
            {
                Id = reader.GetInt32(0),
                WorkerCode = reader.GetString(1),
                Name = reader.GetString(2),
                Role = reader.GetString(3),
                Phone = GetString(reader, 4),
                Email = GetString(reader, 5),
                HireDate = DateTime.Parse(reader.GetString(6)),
                Status = reader.GetString(7)
            });
        }

        return workers;
    }

    private static List<ProjectRecord> LoadProjects(SqliteConnection connection)
    {
        List<ProjectRecord> projects = [];
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, ProjectCode, Name, Location, StartDate, EndDate, Manager, Status, Progress FROM Projects ORDER BY Id;";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            projects.Add(new ProjectRecord
            {
                Id = reader.GetInt32(0),
                ProjectCode = reader.GetString(1),
                Name = reader.GetString(2),
                Location = reader.GetString(3),
                StartDate = DateTime.Parse(reader.GetString(4)),
                EndDate = DateTime.Parse(reader.GetString(5)),
                Manager = GetString(reader, 6),
                Status = reader.GetString(7),
                Progress = reader.GetInt32(8)
            });
        }

        return projects;
    }

    private static List<TaskRecord> LoadTasks(SqliteConnection connection)
    {
        List<TaskRecord> tasks = [];
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TaskCode, Title, ProjectName, AssignedTo, Priority, DueDate, Status FROM Tasks ORDER BY Id;";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            tasks.Add(new TaskRecord
            {
                Id = reader.GetInt32(0),
                TaskCode = reader.GetString(1),
                Title = reader.GetString(2),
                ProjectName = reader.GetString(3),
                AssignedTo = reader.GetString(4),
                Priority = reader.GetString(5),
                DueDate = DateTime.Parse(reader.GetString(6)),
                Status = reader.GetString(7)
            });
        }

        return tasks;
    }

    private static List<AttendanceRecord> LoadAttendance(SqliteConnection connection)
    {
        List<AttendanceRecord> attendance = [];
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, WorkerCode, WorkerName, Role, ProjectName, Date, CheckIn, CheckOut, Status FROM Attendance ORDER BY Id;";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            attendance.Add(new AttendanceRecord
            {
                Id = reader.GetInt32(0),
                WorkerCode = reader.GetString(1),
                WorkerName = reader.GetString(2),
                Role = reader.GetString(3),
                ProjectName = reader.GetString(4),
                Date = DateTime.Parse(reader.GetString(5)),
                CheckIn = GetString(reader, 6),
                CheckOut = GetString(reader, 7),
                Status = reader.GetString(8)
            });
        }

        return attendance;
    }

    private static List<MaterialRecord> LoadMaterials(SqliteConnection connection)
    {
        List<MaterialRecord> materials = [];
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Category, Unit, Quantity, UnitPrice, Status FROM Materials ORDER BY Id;";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            materials.Add(new MaterialRecord
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Category = reader.GetString(2),
                Unit = reader.GetString(3),
                Quantity = reader.GetInt32(4),
                UnitPrice = decimal.Parse(reader.GetString(5), CultureInfo.InvariantCulture),
                Status = reader.GetString(6)
            });
        }

        return materials;
    }

    private static List<IssueReportRecord> LoadIssueReports(SqliteConnection connection)
    {
        List<IssueReportRecord> issueReports = [];
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, ProjectName, ReportedBy, Priority, Status, ReportedDate, Description FROM IssueReports ORDER BY Id;";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            issueReports.Add(new IssueReportRecord
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                ProjectName = reader.GetString(2),
                ReportedBy = reader.GetString(3),
                Priority = reader.GetString(4),
                Status = reader.GetString(5),
                ReportedDate = DateTime.Parse(reader.GetString(6)),
                Description = GetString(reader, 7)
            });
        }

        return issueReports;
    }

    private static void SaveUsers(SqliteConnection connection, SqliteTransaction transaction, IEnumerable<UserAccount> users)
    {
        foreach (UserAccount user in users)
        {
            ExecuteNonQuery(connection, transaction,
                "INSERT INTO Users (Id, Name, Email, PasswordHash, Role, ContactNumber) VALUES ($id, $name, $email, $passwordHash, $role, $contactNumber);",
                ("$id", user.Id), ("$name", user.Name), ("$email", user.Email), ("$passwordHash", user.PasswordHash), ("$role", user.Role.ToString()), ("$contactNumber", user.ContactNumber));
        }
    }

    private static void SaveWorkers(SqliteConnection connection, SqliteTransaction transaction, IEnumerable<WorkerRecord> workers)
    {
        foreach (WorkerRecord worker in workers)
        {
            ExecuteNonQuery(connection, transaction,
                "INSERT INTO Workers (Id, WorkerCode, Name, Role, Phone, Email, HireDate, Status) VALUES ($id, $workerCode, $name, $role, $phone, $email, $hireDate, $status);",
                ("$id", worker.Id), ("$workerCode", worker.WorkerCode), ("$name", worker.Name), ("$role", worker.Role), ("$phone", worker.Phone), ("$email", worker.Email), ("$hireDate", ToDateText(worker.HireDate)), ("$status", worker.Status));
        }
    }

    private static void SaveProjects(SqliteConnection connection, SqliteTransaction transaction, IEnumerable<ProjectRecord> projects)
    {
        foreach (ProjectRecord project in projects)
        {
            ExecuteNonQuery(connection, transaction,
                "INSERT INTO Projects (Id, ProjectCode, Name, Location, StartDate, EndDate, Manager, Status, Progress) VALUES ($id, $projectCode, $name, $location, $startDate, $endDate, $manager, $status, $progress);",
                ("$id", project.Id), ("$projectCode", project.ProjectCode), ("$name", project.Name), ("$location", project.Location), ("$startDate", ToDateText(project.StartDate)), ("$endDate", ToDateText(project.EndDate)), ("$manager", project.Manager), ("$status", project.Status), ("$progress", project.Progress));
        }
    }

    private static void SaveTasks(SqliteConnection connection, SqliteTransaction transaction, IEnumerable<TaskRecord> tasks)
    {
        foreach (TaskRecord task in tasks)
        {
            ExecuteNonQuery(connection, transaction,
                "INSERT INTO Tasks (Id, TaskCode, Title, ProjectName, AssignedTo, Priority, DueDate, Status) VALUES ($id, $taskCode, $title, $projectName, $assignedTo, $priority, $dueDate, $status);",
                ("$id", task.Id), ("$taskCode", task.TaskCode), ("$title", task.Title), ("$projectName", task.ProjectName), ("$assignedTo", task.AssignedTo), ("$priority", task.Priority), ("$dueDate", ToDateText(task.DueDate)), ("$status", task.Status));
        }
    }

    private static void SaveAttendance(SqliteConnection connection, SqliteTransaction transaction, IEnumerable<AttendanceRecord> attendanceRecords)
    {
        foreach (AttendanceRecord record in attendanceRecords)
        {
            ExecuteNonQuery(connection, transaction,
                "INSERT INTO Attendance (Id, WorkerCode, WorkerName, Role, ProjectName, Date, CheckIn, CheckOut, Status) VALUES ($id, $workerCode, $workerName, $role, $projectName, $date, $checkIn, $checkOut, $status);",
                ("$id", record.Id), ("$workerCode", record.WorkerCode), ("$workerName", record.WorkerName), ("$role", record.Role), ("$projectName", record.ProjectName), ("$date", ToDateText(record.Date)), ("$checkIn", record.CheckIn), ("$checkOut", record.CheckOut), ("$status", record.Status));
        }
    }

    private static void SaveMaterials(SqliteConnection connection, SqliteTransaction transaction, IEnumerable<MaterialRecord> materials)
    {
        foreach (MaterialRecord material in materials)
        {
            ExecuteNonQuery(connection, transaction,
                "INSERT INTO Materials (Id, Name, Category, Unit, Quantity, UnitPrice, Status) VALUES ($id, $name, $category, $unit, $quantity, $unitPrice, $status);",
                ("$id", material.Id), ("$name", material.Name), ("$category", material.Category), ("$unit", material.Unit), ("$quantity", material.Quantity), ("$unitPrice", material.UnitPrice.ToString(CultureInfo.InvariantCulture)), ("$status", material.Status));
        }
    }

    private static void SaveIssueReports(SqliteConnection connection, SqliteTransaction transaction, IEnumerable<IssueReportRecord> issueReports)
    {
        foreach (IssueReportRecord issue in issueReports)
        {
            ExecuteNonQuery(connection, transaction,
                "INSERT INTO IssueReports (Id, Title, ProjectName, ReportedBy, Priority, Status, ReportedDate, Description) VALUES ($id, $title, $projectName, $reportedBy, $priority, $status, $reportedDate, $description);",
                ("$id", issue.Id), ("$title", issue.Title), ("$projectName", issue.ProjectName), ("$reportedBy", issue.ReportedBy), ("$priority", issue.Priority), ("$status", issue.Status), ("$reportedDate", ToDateText(issue.ReportedDate)), ("$description", issue.Description));
        }
    }

    private static void ExecuteNonQuery(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        string commandText,
        params (string Name, object? Value)[] parameters)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        command.Transaction = transaction;

        foreach ((string name, object? value) in parameters)
        {
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        }

        command.ExecuteNonQuery();
    }

    private static string GetString(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static string ToDateText(DateTime date)
    {
        return date.Date.ToString("yyyy-MM-dd");
    }
}
