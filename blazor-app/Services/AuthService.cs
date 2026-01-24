using BlazorApp.Models;
using LiteDB;
using System.Security.Cryptography;
using System.Text;

namespace BlazorApp.Services;

public class AuthService
{
    private readonly string _dbPath;
    private User? _currentUser;
    public event Action? OnAuthStateChanged;

    public bool IsAuthenticated => _currentUser != null;
    public User? CurrentUser => _currentUser;

    public AuthService(IWebHostEnvironment env)
    {
        var dataPath = Path.Combine(env.ContentRootPath, "Data");
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        _dbPath = Path.Combine(dataPath, "users.db");
    }

    public bool Register(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        using var db = new LiteDatabase(_dbPath);
        var users = db.GetCollection<User>("users");

        // Check if username already exists
        if (users.FindOne(u => u.Username == username) != null)
        {
            return false;
        }

        var user = new User
        {
            Username = username,
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.Now
        };

        users.Insert(user);
        return true;
    }

    public bool Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        using var db = new LiteDatabase(_dbPath);
        var users = db.GetCollection<User>("users");

        var user = users.FindOne(u => u.Username == username);
        if (user == null)
        {
            return false;
        }

        if (VerifyPassword(password, user.PasswordHash))
        {
            _currentUser = user;
            OnAuthStateChanged?.Invoke();
            return true;
        }

        return false;
    }

    public void Logout()
    {
        _currentUser = null;
        OnAuthStateChanged?.Invoke();
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashedPassword = HashPassword(password);
        return hashedPassword == hash;
    }
}
