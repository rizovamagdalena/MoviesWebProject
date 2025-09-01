using Microsoft.Extensions.Options;
using MoviesWEB.Models;
using MoviesWEB.Models.System;
using MoviesWEB.Service.Interface;
using Newtonsoft.Json;
using System.Text;

namespace MoviesWEB.Service.Implementation
{
    public class ChatBotService : IChatBotService
    {
        private readonly string _apiBaseUrl;
        private readonly HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatBotService(IOptions<DbSettings> settings, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _client = httpClientFactory.CreateClient();
            _apiBaseUrl = settings.Value.DbApi ?? throw new ArgumentNullException(nameof(settings.Value.DbApi));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<Faq>> GetAllFaqAsync()
        {
            var response = await _client.GetAsync($"{_apiBaseUrl}/api/chatbot/faqs");
            if (!response.IsSuccessStatusCode) return new List<Faq>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Faq>>(json) ?? new List<Faq>();
        }

        public async Task<Faq?> GetFaqByIdAsync(int id)
        {
            var response = await _client.GetAsync($"{_apiBaseUrl}/api/chatbot/faqs/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Faq>(json);
        }

        public async Task AddFaqAsync(Faq faq)
        {
            var json = JsonConvert.SerializeObject(faq);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _client.PostAsync($"{_apiBaseUrl}/api/chatbot/faq", content);
        }

        public async Task UpdateFaqAsync(Faq faq)
        {
            var json = JsonConvert.SerializeObject(faq);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _client.PutAsync($"{_apiBaseUrl}/api/chatbot/faq/{faq.Id}", content);
        }

        public async Task DeleteFaqAsync(int id)
        {
            await _client.DeleteAsync($"{_apiBaseUrl}/api/chatbot/faq/{id}");
        }

        public async Task<Faq?> GetClosestMatchToFaqAsync(string userQuestion)
        {
            var json = JsonConvert.SerializeObject(new { Question = userQuestion });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{_apiBaseUrl}/api/chatbot/ask", content);
            if (!response.IsSuccessStatusCode) return null;

            var resJson = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(resJson);
            return new Faq { Answer = data?["answer"] ?? string.Empty };
        }
    }
}
