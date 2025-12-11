using System.Net.Http.Json;
using CampusConnectHub.Shared.DTOs;

namespace CampusConnectHub.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    await _localStorage.SetItemAsync("authToken", authResponse.Token);
                    await _localStorage.SetItemAsync("user", authResponse.User);
                    return authResponse;
                }
            }
            else
            {
                // Try to read error message from response
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Login failed: {response.StatusCode}");
            }
        }
        catch (HttpRequestException)
        {
            // Re-throw to be caught by the UI
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while connecting to the server. Please check your connection and try again.", ex);
        }
        
        return null;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (authResponse != null)
                {
                    await _localStorage.SetItemAsync("authToken", authResponse.Token);
                    await _localStorage.SetItemAsync("user", authResponse.User);
                    return authResponse;
                }
            }
            else
            {
                // Try to read error message from response
                var errorContent = await response.Content.ReadAsStringAsync();
                var statusCode = (int)response.StatusCode;
                
                if (statusCode == 400)
                {
                    throw new HttpRequestException($"Registration failed: Email already registered");
                }
                else if (statusCode == 401)
                {
                    throw new HttpRequestException($"Registration failed: Unauthorized");
                }
                else
                {
                    throw new HttpRequestException($"Registration failed: {response.StatusCode}");
                }
            }
        }
        catch (HttpRequestException)
        {
            // Re-throw to be caught by the UI
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while connecting to the server. Please check your connection and try again.", ex);
        }
        
        return null;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("user");
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }

    public async Task<UserResponse?> GetUserAsync()
    {
        return await _localStorage.GetItemAsync<UserResponse>("user");
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}

