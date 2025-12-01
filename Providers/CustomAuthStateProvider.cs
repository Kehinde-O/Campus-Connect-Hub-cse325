using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace DigitalBulletinBoard.Providers
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Check if user is logged in
            var isLoggedIn = localStorage.ContainsKey("auth_user");

            if (!isLoggedIn)
                return Task.FromResult(new AuthenticationState(_anonymous));

            var username = localStorage["auth_user"];

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username)
            }, "FakeAuth");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }

        // LOGIN
        public Task LoginUser(string username)
        {
            localStorage["auth_user"] = username;

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return Task.CompletedTask;
        }

        // LOGOUT
        public Task LogoutUser()
        {
            if (localStorage.ContainsKey("auth_user"))
                localStorage.Remove("auth_user");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            return Task.CompletedTask;
        }

        // Simulated local storage (memory only)
        private static Dictionary<string, string> localStorage = new();
    }
}
