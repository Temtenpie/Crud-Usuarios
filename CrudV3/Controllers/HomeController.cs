using CrudV3.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace CrudV3.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _supabaseUrl = configuration["Supabase:Url"] ?? string.Empty;
            _supabaseKey = configuration["Supabase:Key"] ?? string.Empty;

            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
        }

        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                var user = await ValidateUserAsync(model);
                if (user != null)
                {
                    TempData["Username"] = user.Username;
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    model.ErrorMessage = "Usuario o contraseña incorrectos";
                    return View("Index", model);
                }
            }
            catch (Exception)
            {
                model.ErrorMessage = "Error del sistema. Intenta nuevamente.";
                return View("Index", model);
            }
        }

        public IActionResult Dashboard()
        {
            if (TempData["Username"] == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Username = TempData["Username"];
            return View();
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Index");
        }

        private async Task<UserModel?> ValidateUserAsync(LoginViewModel model)
        {
            try
            {
                string url = $"{_supabaseUrl}/rest/v1/users?username=eq.{model.Username}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<UserModel[]>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                    });

                    var user = users?.FirstOrDefault();
                    if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                    {
                        return user;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}