using BusinessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace App.Services
{
    internal class HttpService
    {
        private readonly HttpClient _httpClient;
        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }
        public async Task<User> PostUserLogin(string email, string password)
        {
            var json = JsonSerializer.Serialize(new {email,password });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("user/login",content);
            if (response.IsSuccessStatusCode)
            {
                return response.RequestMessage != null && response.RequestMessage.Content != null
                    ? JsonSerializer.Deserialize<User>(await response.Content.ReadAsStringAsync())
                    : null;
            }
            return null;
        }
    }
}
