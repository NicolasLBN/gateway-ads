using LiteDB;

namespace BlazorApp.Models;

public class AuthUser
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
