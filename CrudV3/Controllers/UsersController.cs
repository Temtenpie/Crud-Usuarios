using CrudV3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CrudV3.Controllers
{
    public class UsersController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;

        public UsersController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _supabaseUrl = configuration["Supabase:Url"] ?? string.Empty;
            _supabaseKey = configuration["Supabase:Key"] ?? string.Empty;

            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
        }

        public async Task<IActionResult> Index()
        {
            var users = await GetUsersAsync();
            return View(users);
        }

        private async Task<List<UserModel>> GetUsersAsync()
        {
            try
            {
                string url = $"{_supabaseUrl}/rest/v1/users?select=*&order=id";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<UserModel[]>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });

                    return users?.ToList() ?? new List<UserModel>();
                }

                return new List<UserModel>();
            }
            catch
            {
                return new List<UserModel>();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserModel user)
        {
            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);


                object userToSerialize = new
                    {
                        username = user.Username,
                        email = user.Email,
                        password_hash = user.PasswordHash,
                        is_active = user.IsActive,
                        created_at = user.CreatedAt,
                        last_login = user.LastLogin
                    };
        

                var json = JsonSerializer.Serialize(userToSerialize, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_supabaseUrl}/rest/v1/users", content);

                return Json(new { success = response.IsSuccessStatusCode });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserModel user)
        {
            try
            {
                if (!string.IsNullOrEmpty(user.PasswordHash))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                }

                //se crea un nuevo objeto con una copia identica al UserModel pero sin el ID ya que no se debe enviar en el json
                object userToSerialize = new
                {
                    username = user.Username,
                    email = user.Email,
                    password_hash = user.PasswordHash,
                    is_active = user.IsActive,
                    created_at = user.CreatedAt,
                    last_login = user.LastLogin
                };


                var json = JsonSerializer.Serialize(userToSerialize, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                //en esta url se esta enviando el id del usuario a modificar {user.Id} y el json con las modificaciones
                var response = await _httpClient.PatchAsync($"{_supabaseUrl}/rest/v1/users?id=eq.{user.Id}", content);
                //en la base de datos no esta realizando ninguna accion con el update revisar
                return Json(new { success = response.IsSuccessStatusCode });
                
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_supabaseUrl}/rest/v1/users?id=eq.{id}");
                return Json(new { success = response.IsSuccessStatusCode });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}