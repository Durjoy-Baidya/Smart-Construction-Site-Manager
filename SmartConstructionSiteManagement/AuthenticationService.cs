using System.Security.Cryptography;
using System.Text;

namespace SmartConstructionSiteManagement;

public enum UserRole
{
    Admin,
    SiteManager,
    Worker
}

public class UserAccount
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
}

public class AuthenticationService
{
    private readonly IAppDataStore appDataStore;

    public AuthenticationService(IAppDataStore appDataStore)
    {
        this.appDataStore = appDataStore;
    }

    public UserAccount? Login(string email, string password)
    {
        ApplicationData data = LoadOrCreateData();
        string normalizedEmail = email.Trim().ToLowerInvariant();
        string passwordHash = PasswordHasher.Hash(password);

        return data.Users.FirstOrDefault(user =>
            user.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            user.PasswordHash == passwordHash);
    }

    public string? ResetPassword(string email)
    {
        ApplicationData data = LoadOrCreateData();
        string? defaultPassword = GetDefaultPassword(email);

        if (defaultPassword == null)
        {
            return null;
        }

        UserAccount? user = data.Users.FirstOrDefault(user =>
            user.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user == null)
        {
            return null;
        }

        user.PasswordHash = PasswordHasher.Hash(defaultPassword);
        appDataStore.Save(data);
        return defaultPassword;
    }

    public ApplicationData LoadOrCreateData()
    {
        ApplicationData data = appDataStore.Load() ?? new ApplicationData();

        if (!SeedData.HasAnyData(data))
        {
            data = SeedData.CreateDefaultData();
            appDataStore.Save(data);
            return data;
        }

        if (data.Users.Count == 0)
        {
            data.Users.AddRange(SeedData.CreateDefaultUsers());
            appDataStore.Save(data);
        }

        return data;
    }

    private static string? GetDefaultPassword(string email)
    {
        return email.Trim().ToLowerInvariant() switch
        {
            "admin@site.com" => "admin123",
            "manager@site.com" => "manager123",
            "worker@site.com" => "worker123",
            _ => null
        };
    }
}

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes);
    }
}
