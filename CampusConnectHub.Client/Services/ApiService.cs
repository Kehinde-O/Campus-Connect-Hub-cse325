using System.Net.Http.Headers;
using System.Net.Http.Json;
using CampusConnectHub.Client.Services;
using CampusConnectHub.Shared.DTOs;

namespace CampusConnectHub.Client.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ApiService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
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

    // News API
    public async Task<PagedResponse<NewsPostDto>> GetNewsAsync(int pageNumber = 1, int pageSize = 10)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<PagedResponse<NewsPostDto>>(
            $"api/news?pageNumber={pageNumber}&pageSize={pageSize}") ?? new PagedResponse<NewsPostDto>();
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
    public async Task<List<EventDto>> GetEventsAsync(bool upcomingOnly = true)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<List<EventDto>>(
            $"api/events?upcomingOnly={upcomingOnly}") ?? new List<EventDto>();
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
}

