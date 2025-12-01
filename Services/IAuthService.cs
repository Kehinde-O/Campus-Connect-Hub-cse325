public interface IAuthService
{
    bool IsAuthenticated { get; }
    string? CurrentUser { get; }

    event Action? OnChange;

    Task<bool> LoginAsync(string username, string password);
    Task<bool> RegisterAsync(string username, string password);
    void Logout();
}
