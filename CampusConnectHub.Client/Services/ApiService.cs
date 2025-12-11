using System.Net.Http.Headers;
using System.Net.Http.Json;
using CampusConnectHub.Client.Services;
using CampusConnectHub.Shared.DTOs;
using Microsoft.AspNetCore.Components;

namespace CampusConnectHub.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly AuthService _authService;

    public ApiService(HttpClient httpClient, ILocalStorageService localStorage, NavigationManager navigationManager, AuthService authService)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _authService = authService;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    private async Task HandleUnauthorizedAsync()
    {
        await _authService.LogoutAsync();
        var currentUrl = _navigationManager.Uri;
        _navigationManager.NavigateTo($"/login?returnUrl={Uri.EscapeDataString(currentUrl)}");
    }

    private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response) where T : class
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await HandleUnauthorizedAsync();
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    private async Task<T?> HandleGetAsync<T>(string url) where T : class
    {
        try
        {
            await SetAuthHeaderAsync();
            var response = await _httpClient.GetAsync(url);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await HandleUnauthorizedAsync();
                return null;
            }
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            await HandleUnauthorizedAsync();
            return null;
        }
    }

    // News API
    public async Task<PagedResponse<NewsPostDto>> GetNewsAsync(int pageNumber = 1, int pageSize = 10, string? search = null)
    {
        await SetAuthHeaderAsync();
        var queryParams = $"pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParams += $"&search={Uri.EscapeDataString(search)}";
        }
        return await _httpClient.GetFromJsonAsync<PagedResponse<NewsPostDto>>(
            $"api/news?{queryParams}") ?? new PagedResponse<NewsPostDto>();
    }

    public async Task<NewsPostDto?> GetNewsPostAsync(int id)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<NewsPostDto>($"api/news/{id}");
    }

    public async Task<bool> CreateNewsPostAsync(CreateNewsPostDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/news", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateNewsPostAsync(int id, CreateNewsPostDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/news/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteNewsPostAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/news/{id}");
        return response.IsSuccessStatusCode;
    }

    // Events API
    public async Task<List<EventDto>> GetEventsAsync(bool upcomingOnly = true, string? search = null, string? location = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        await SetAuthHeaderAsync();
        var queryParams = $"upcomingOnly={upcomingOnly}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParams += $"&search={Uri.EscapeDataString(search)}";
        }
        if (!string.IsNullOrWhiteSpace(location))
        {
            queryParams += $"&location={Uri.EscapeDataString(location)}";
        }
        if (startDate.HasValue)
        {
            queryParams += $"&startDate={startDate.Value:yyyy-MM-ddTHH:mm:ssZ}";
        }
        if (endDate.HasValue)
        {
            queryParams += $"&endDate={endDate.Value:yyyy-MM-ddTHH:mm:ssZ}";
        }
        return await _httpClient.GetFromJsonAsync<List<EventDto>>(
            $"api/events?{queryParams}") ?? new List<EventDto>();
    }

    public async Task<EventDto?> GetEventAsync(int id)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<EventDto>($"api/events/{id}");
    }

    public async Task<bool> CreateEventAsync(CreateEventDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/events", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateEventAsync(int id, CreateEventDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/events/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteEventAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/events/{id}");
        return response.IsSuccessStatusCode;
    }

    // RSVP API
    public async Task<bool> RSVPToEventAsync(int eventId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/eventrsvp/{eventId}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CancelRSVPAsync(int eventId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/eventrsvp/{eventId}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<RSVPDto>> GetMyRSVPsAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<RSVPDto>>("api/eventrsvp/my-rsvps") ?? new List<RSVPDto>();
    }

    // Resources API
    public async Task<List<ResourceDto>> GetResourcesAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<ResourceDto>>("api/resources") ?? new List<ResourceDto>();
    }

    public async Task<List<ResourceDto>> GetAdminResourcesAsync()
    {
        var result = await HandleGetAsync<List<ResourceDto>>("api/resources/admin/all");
        return result ?? new List<ResourceDto>();
    }

    public async Task<bool> CreateResourceAsync(CreateResourceDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/resources", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateResourceAsync(int id, CreateResourceDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/resources/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteResourceAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/resources/{id}");
        return response.IsSuccessStatusCode;
    }

    // Admin API
    public async Task<AdminDashboardDto?> GetAdminDashboardAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<AdminDashboardDto>("api/admin/dashboard");
    }

    public async Task<AnalyticsDto?> GetAnalyticsAsync()
    {
        return await HandleGetAsync<AnalyticsDto>("api/admin/analytics");
    }

    // Profile API
    public async Task<AuthResponse?> UpdateProfileAsync(UpdateProfileDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync("api/auth/profile", dto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponse>();
        }
        return null;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", dto);
        return response.IsSuccessStatusCode;
    }

    // User Management API
    public async Task<PagedResponse<UserDto>> GetUsersAsync(int pageNumber = 1, int pageSize = 20, string? search = null, string? role = null)
    {
        var queryParams = $"pageNumber={pageNumber}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParams += $"&search={Uri.EscapeDataString(search)}";
        }
        if (!string.IsNullOrWhiteSpace(role))
        {
            queryParams += $"&role={Uri.EscapeDataString(role)}";
        }
        var result = await HandleGetAsync<PagedResponse<UserDto>>($"api/users?{queryParams}");
        return result ?? new PagedResponse<UserDto>();
    }

    public async Task<UserDto?> GetUserAsync(int id)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
    }

    public async Task<bool> UpdateUserRoleAsync(int id, UpdateUserRoleDto dto)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/users/{id}/role", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/users/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<UserActivityDto?> GetUserActivityAsync(int id)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<UserActivityDto>($"api/users/{id}/activity");
    }

    // Event Attendees API
    public async Task<List<EventAttendeeDto>> GetEventAttendeesAsync(int eventId)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<EventAttendeeDto>>($"api/events/{eventId}/attendees") ?? new List<EventAttendeeDto>();
    }

    public async Task<byte[]?> ExportEventAttendeesAsync(int eventId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/events/{eventId}/attendees/export");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }
        return null;
    }
}

