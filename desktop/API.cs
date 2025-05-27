using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace BookCatalog
{
    public static class API
    {
        static readonly string URL = "http://localhost:9000/api/";
        static readonly HttpClient http = new HttpClient();
        static string Token { get; set; } = null;
        public static bool LoggedIn => Token != null;

        private static void SetAuthorizationHeader()
        {
            http.DefaultRequestHeaders.Authorization = null;
            if (LoggedIn)
            {
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);
            }
        }

        static async Task<T> PostAsync<T>(string action, object payload)
        {
            string body = null;
            try
            {
                string jsonPayload = JsonConvert.SerializeObject(payload);
                HttpContent reqBody = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await http.PostAsync(URL + action, reqBody);
                body = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(body);
            }
            catch (HttpRequestException)
            {
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        MsgResponse msgResponse = JsonConvert.DeserializeObject<MsgResponse>(body);
                        throw new Exception(msgResponse.Message ?? "An unexpected error occurred. Status: " + (JsonConvert.DeserializeObject<dynamic>(body)?.statusCode ?? "Unknown"));
                    }
                    catch (JsonException)
                    {
                        throw new Exception("An error occurred while processing the request. The response was not in the expected format.");
                    }
                }
                throw new Exception("An unexpected network error occurred.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

        static async Task<T> PutAsync<T>(string action, object payload)
        {
            string body = null;
            try
            {
                string jsonPayload = JsonConvert.SerializeObject(payload);
                HttpContent reqBody = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await http.PutAsync(URL + action, reqBody);
                body = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(body);
            }
            catch (HttpRequestException)
            {
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        MsgResponse msgResponse = JsonConvert.DeserializeObject<MsgResponse>(body);
                        throw new Exception(msgResponse.Message ?? "An unexpected error occurred. Status: " + (JsonConvert.DeserializeObject<dynamic>(body)?.statusCode ?? "Unknown"));
                    }
                    catch (JsonException)
                    {
                        throw new Exception("An error occurred while processing the request. The response was not in the expected format.");
                    }
                }
                throw new Exception("An unexpected network error occurred.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

        static async Task<T> GetAsync<T>(string action)
        {
            string body = null;
            try
            {
                HttpResponseMessage response = await http.GetAsync(URL + action);
                body = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(body);
            }
            catch (HttpRequestException)
            {
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        MsgResponse msgResponse = JsonConvert.DeserializeObject<MsgResponse>(body);
                        throw new Exception(msgResponse.Message ?? "An unexpected error occurred. Status: " + (JsonConvert.DeserializeObject<dynamic>(body)?.statusCode ?? "Unknown"));
                    }
                    catch (JsonException)
                    {
                        throw new Exception("An error occurred while processing the request. The response was not in the expected format.");
                    }
                }
                throw new Exception("An unexpected network error occurred.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

        static async Task DeleteAsyncHttp(string action)
        {
            string body = null;
            try
            {
                HttpResponseMessage response = await http.DeleteAsync(URL + action);
                body = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                if (!string.IsNullOrEmpty(body))
                {
                    try
                    {
                        MsgResponse msgResponse = JsonConvert.DeserializeObject<MsgResponse>(body);
                        throw new Exception(msgResponse.Message ?? "An unexpected error occurred. Status: " + (JsonConvert.DeserializeObject<dynamic>(body)?.statusCode ?? "Unknown"));
                    }
                    catch (JsonException)
                    {
                        throw new Exception("An error occurred while processing the request. The response was not in the expected format.");
                    }
                }
                throw new Exception("An unexpected network error occurred.");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }
        }

        public static async Task<bool> Login(string name, string password)
        {
            TokenBody tokenResponse = await PostAsync<TokenBody>("login", new LoginBody(name, password));
            Token = tokenResponse.Token;
            SetAuthorizationHeader();
            return true;
        }
        public static async Task<bool> Register(string name, string password)
        {
            TokenBody tokenResponse = await PostAsync<TokenBody>("register", new LoginBody(name, password));
            Token = tokenResponse.Token;
            SetAuthorizationHeader();
            return true;
        }
        public static void LogOut()
        {
            Token = null;
            SetAuthorizationHeader();
        }
        public static async Task<UserInfo> GetUserInfo()
        {
            return await GetAsync<UserInfo>("me");
        }
        public static async Task<List<Book>> GetBooks(string searchTerm = null, int? categoryId = null)
        {
            string query = "books";
            List<string> queryParams = new List<string>();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                queryParams.Add($"search={Uri.EscapeDataString(searchTerm)}");
            }
            if (categoryId.HasValue)
            {
                return await GetAsync<List<Book>>($"books/category/{categoryId.Value}");
            }
            if (queryParams.Any())
            {
                query += "?" + string.Join("&", queryParams);
            }
            return await GetAsync<List<Book>>(query);
        }
        public static async Task DeleteBook(int id)
        {
            await DeleteAsyncHttp($"books/{id}");
        }
        public static async Task<Book> SendBook(Book book, bool edit)
        {
            var payload = new
            {
                author = book.Author,
                title = book.Title,
                details = book.Details,
                categoryIds = book.Categories?.Select(c => c.Id).ToList() ?? new List<int>()
            };

            if (edit)
            {
                return await PutAsync<Book>($"books/{book.Id}", payload);
            }
            else
            {
                return await PostAsync<Book>("books", payload);
            }
        }

        public static async Task<List<Category>> GetCategories()
        {
            return await GetAsync<List<Category>>("categories");
        }

        public static async Task<Category> CreateCategory(string name)
        {
            return await PostAsync<Category>("categories", new { name = name });
        }

        public static async Task<Category> UpdateCategory(int id, string name)
        {
            return await PutAsync<Category>($"categories/{id}", new { name = name });
        }

        public static async Task DeleteCategory(int id)
        {
            await DeleteAsyncHttp($"categories/{id}");
        }

        public static async Task<List<CategoryStat>> GetCategoryStats()
        {
            return await GetAsync<List<CategoryStat>>("categories/stats");
        }

        public static async Task<Book> AddCategoryToBook(int bookId, int categoryId)
        {
            return await PutAsync<Book>($"books/{bookId}/category", new { categoryId = categoryId });
        }
    }

    public struct LoginBody
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        public LoginBody(string name, string password)
        {
            Name = name;
            Password = password;
        }
    }

    public struct TokenBody
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }

    public struct MsgResponse
    {
        [JsonProperty("msg")]
        public string Message { get; set; }
    }

    public struct UserInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}