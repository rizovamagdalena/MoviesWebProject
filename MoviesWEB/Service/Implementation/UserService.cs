using Microsoft.Extensions.Options;
using MoviesWEB.Models;
using MoviesWEB.Models.System;
using MoviesWEB.Service.Interface;
using Newtonsoft.Json;
using System.Runtime;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MoviesWEB.Service.Implementation
{
    public class UserService : IUserService
    {
        private readonly string _apiBaseUrl;
        private readonly HttpClient _client;

        public UserService(IOptions<DbSettings> settings, IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _apiBaseUrl = settings.Value.DbApi ?? throw new ArgumentNullException(nameof(settings.Value.DbApi));
        }

        public async Task<LoginResponse> CheckUserCredidentals(LoginRequest loginRequest)
        {
            LoginResponse loginResponse = new LoginResponse();

            HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");
            using (var response = await _client.PostAsync($"{_apiBaseUrl}/api/Account/Login", requestContent))
            {
                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    loginResponse = JsonConvert.DeserializeObject<LoginResponse>(apiResponse);
                }
                else
                {
                    return null;
                }
            }

            return loginResponse;
        }

        public async Task<bool> LogoutUser(string username)
        {
            var json = JsonConvert.SerializeObject(username);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{_apiBaseUrl}/api/account/logout", content);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> TryRegisterRequest(RegisterRequest registerRequest)
        {
            bool loginResponse;

            HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(registerRequest), Encoding.UTF8, "application/json");
            using (var response = await _client.PostAsync($"{_apiBaseUrl}/api/Account/Register", requestContent))
            {
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> UpdateUserProfileAsync(UserProfile userProfile)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(userProfile.Name ?? ""), "Name" },
                { new StringContent(userProfile.Phone ?? ""), "Phone" },
                { new StringContent(userProfile.Username ?? ""), "Username" }
            };

            var response = await _client.PutAsync($"{_apiBaseUrl}/api/Users", content);
            return response.IsSuccessStatusCode;
        }
    }
}
