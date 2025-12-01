public class AuthService : IAuthService
{
    private bool _auth;
    private string? _user;

    public bool IsAuthenticated => _auth;
    public string? CurrentUser => _user;

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();

    public Task<bool> LoginAsync(string username, string password)
    {
        // Replace with real validation later
        if (username == "admin" && password == "123")
        {
            _auth = true;
            _user = username;

            NotifyStateChanged();
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<bool> RegisterAsync(string username, string password)
    {
        // Simplest example — change later
        _auth = true;
        _user = username;

        NotifyStateChanged();
        return Task.FromResult(true);
    }

    public void Logout()
    {
        _auth = false;
        _user = null;

        NotifyStateChanged();
    }
}
