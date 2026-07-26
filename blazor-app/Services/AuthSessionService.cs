using BlazorApp.Models;

namespace BlazorApp.Services;

/// <summary>Per-circuit session for the signed-in operator.</summary>
public class AuthSessionService
{
    private AuthUser? _currentUser;

    public event Action? Changed;

    public AuthUser? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public string? Username => _currentUser?.Username;

    public void SetUser(AuthUser user)
    {
        _currentUser = new AuthUser
        {
            Id = user.Id,
            Username = user.Username,
            CreatedAt = user.CreatedAt
            // PasswordHash intentionally omitted from session
        };
        Changed?.Invoke();
    }

    public void Clear()
    {
        _currentUser = null;
        Changed?.Invoke();
    }
}
