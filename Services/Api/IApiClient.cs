using GMLSSystem.Shared.Models;

namespace GMLSSystem.Services.Api
{
    public interface IApiClient
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task<T> PatchAsync<T>(string endpoint, object data);
        Task<bool> DeleteAsync(string endpoint);

        // NEW: JWT Authentication methods
        Task<ApiResponse<LoginResponseDto>> LoginAsync(string username, string password);
        void SetToken(string token);
        void ClearToken();
    }
}