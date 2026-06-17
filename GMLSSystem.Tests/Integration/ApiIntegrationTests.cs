using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace GMLSSystem.Tests.Integration
{
    public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public async Task GetContracts_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/contracts");

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginData = new { username = "admin@glms.com", password = "Admin@123" };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Log the response for debugging
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Content: {responseContent}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(responseContent, _jsonOptions);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.False(string.IsNullOrEmpty(result.Data.Token));
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginData = new { username = "invalid", password = "wrong" };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Auth/login", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetContracts_WithValidToken_ReturnsContracts()
        {
            // Arrange - First login to get token
            var token = await GetAuthToken();

            // Create request with token
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/contracts");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<List<ContractDto>>>(json, _jsonOptions);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task CreateContract_WithValidData_ReturnsCreatedContract()
        {
            // Arrange
            var token = await GetAuthToken();

            var contract = new
            {
                clientId = 1,
                description = "Integration Test Contract",
                startDate = DateTime.UtcNow.AddDays(1),
                endDate = DateTime.UtcNow.AddDays(365),
                serviceLevel = "Standard",
                region = "Johannesburg",
                status = "Draft"
            };

            var content = new StringContent(JsonSerializer.Serialize(contract), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/contracts")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<ContractDto>>(json, _jsonOptions);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.StartsWith("CTR-", result.Data.ContractNumber);
        }

        [Fact]
        public async Task UpdateContractStatus_WithValidStatus_ReturnsSuccess()
        {
            // Arrange
            var token = await GetAuthToken();

            // First create a contract
            var contractId = await CreateTestContract(token);

            var updateDto = new { status = "Active" };
            var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/contracts/{contractId}/status")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<object>>(json, _jsonOptions);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Contains("updated", result.Message);
        }

        [Fact]
        public async Task CreateServiceRequest_WithValidData_ReturnsCreatedRequest()
        {
            // Arrange
            var token = await GetAuthToken();

            // First create a contract
            var contractId = await CreateTestContract(token);

            // Activate the contract
            await UpdateContractStatus(token, contractId, "Active");

            var requestData = new
            {
                contractId = contractId,
                description = "Integration Test Service Request",
                costUSD = 500.00m
            };

            var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/servicerequests")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<ServiceRequestDto>>(json, _jsonOptions);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.StartsWith("SRQ-", result.Data.RequestNumber);
            Assert.True(result.Data.CostZAR > 0);
        }

        // ===== Helper Methods =====

        private async Task<string> GetAuthToken()
        {
            var loginData = new { username = "admin@glms.com", password = "Admin@123" };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/Auth/login", content);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<LoginResponseDto>>(json, _jsonOptions);

            return result?.Data?.Token ?? string.Empty;
        }

        private async Task<int> CreateTestContract(string token)
        {
            var contract = new
            {
                clientId = 1,
                description = "Test Contract for Integration",
                startDate = DateTime.UtcNow.AddDays(1),
                endDate = DateTime.UtcNow.AddDays(365),
                serviceLevel = "Standard",
                region = "Johannesburg",
                status = "Draft"
            };

            var content = new StringContent(JsonSerializer.Serialize(contract), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/contracts")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<ContractDto>>(json, _jsonOptions);

            return result?.Data?.ContractId ?? 0;
        }

        private async Task UpdateContractStatus(string token, int contractId, string status)
        {
            var updateDto = new { status = status };
            var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"/api/contracts/{contractId}/status")
            {
                Content = content
            };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            await _client.SendAsync(request);
        }
    }

    // ===== DTOs for Testing =====

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public int? ClientId { get; set; }
    }

    public class ContractDto
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ServiceLevel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? Region { get; set; }
    }

    public class ServiceRequestDto
    {
        public int ServiceRequestId { get; set; }
        public int ContractId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}