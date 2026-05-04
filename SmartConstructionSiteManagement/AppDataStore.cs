using System.Text.Json;

namespace SmartConstructionSiteManagement;

public class ApplicationData
{
    public List<UserAccount> Users { get; set; } = [];
    public List<WorkerRecord> Workers { get; set; } = [];
    public List<ProjectRecord> Projects { get; set; } = [];
    public List<TaskRecord> Tasks { get; set; } = [];
    public List<AttendanceRecord> AttendanceRecords { get; set; } = [];
    public List<MaterialRecord> Materials { get; set; } = [];
    public List<IssueReportRecord> IssueReports { get; set; } = [];
}

public interface IAppDataStore
{
    ApplicationData? Load();
    void Save(ApplicationData data);
}

public class JsonAppDataStore : IAppDataStore
{
    private readonly string dataFilePath;
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true
    };

    public JsonAppDataStore()
    {
        string dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartConstructionSiteManagement");

        Directory.CreateDirectory(dataDirectory);
        dataFilePath = Path.Combine(dataDirectory, "app-data.json");
    }

    public ApplicationData? Load()
    {
        if (!File.Exists(dataFilePath))
        {
            return null;
        }

        string json = File.ReadAllText(dataFilePath);
        return JsonSerializer.Deserialize<ApplicationData>(json, jsonOptions);
    }

    public void Save(ApplicationData data)
    {
        string json = JsonSerializer.Serialize(data, jsonOptions);
        File.WriteAllText(dataFilePath, json);
    }
}
