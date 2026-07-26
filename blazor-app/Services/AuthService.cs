using BlazorApp.Models;
using LiteDB;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp.Services;

public class AuthService : IDisposable
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<AuthUser> _users;
    private readonly PasswordHasher<AuthUser> _hasher = new();
    private readonly object _lock = new();

    public AuthService(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "auth.db");
        _db = new LiteDatabase(dbPath);
        _users = _db.GetCollection<AuthUser>("users");
        _users.EnsureIndex(u => u.Username, unique: true);
    }

    public AuthResult Register(string username, string password)
    {
        username = Normalize(username);
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
            return AuthResult.Fail("Username must be at least 3 characters.");

        if (string.IsNullOrEmpty(password) || password.Length < 6)
            return AuthResult.Fail("Password must be at least 6 characters.");

        lock (_lock)
        {
            if (_users.Exists(u => u.Username == username))
                return AuthResult.Fail("This username is already taken.");

            var user = new AuthUser { Username = username };
            user.PasswordHash = _hasher.HashPassword(user, password);
            _users.Insert(user);
            return AuthResult.Ok(user);
        }
    }

    public AuthResult Login(string username, string password)
    {
        username = Normalize(username);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            return AuthResult.Fail("Enter username and password.");

        lock (_lock)
        {
            var user = _users.FindOne(u => u.Username == username);
            if (user == null)
                return AuthResult.Fail("Invalid username or password.");

            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verify == PasswordVerificationResult.Failed)
                return AuthResult.Fail("Invalid username or password.");

            if (verify == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _hasher.HashPassword(user, password);
                _users.Update(user);
            }

            return AuthResult.Ok(user);
        }
    }

    private static string Normalize(string username) => username.Trim().ToLowerInvariant();

    public void Dispose() => _db.Dispose();
}

public sealed class AuthResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public AuthUser? User { get; init; }

    public static AuthResult Ok(AuthUser user) => new() { Success = true, User = user };
    public static AuthResult Fail(string error) => new() { Success = false, Error = error };
}
