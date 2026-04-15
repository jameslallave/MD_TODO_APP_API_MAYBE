using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ToDo_App;

public static class ApiService
{
    private static readonly string BaseUrl = "https://todo-list.dcism.org";
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new() 
    { 
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Safely convert a JsonElement to an integer
    /// </summary>
    private static bool TryGetInt(JsonElement element, out int value)
    {
        try
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.TryGetInt32(out value);
            }
            else if (element.ValueKind == JsonValueKind.String)
            {
                return int.TryParse(element.GetString(), out value);
            }
        }
        catch { }
        
        value = 0;
        return false;
    }

    #region Sign Up
    /// <summary>
    /// Register a new user account
    /// </summary>
    public static async Task<ApiResponse<string>> SignUpAsync(string firstName, string lastName, string email, string password, string confirmPassword)
    {
        try
        {
            var payload = new { first_name = firstName, last_name = lastName, email = email, password = password, confirm_password = confirmPassword };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync($"{BaseUrl}/signup_action.php", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("status", out var statusElem) || !TryGetInt(statusElem, out int status))
            {
                return new ApiResponse<string> { Success = false, StatusCode = 500, Message = "Invalid response format: missing or invalid status", Data = null };
            }

            string message = "Unknown error";
            if (root.TryGetProperty("message", out var messageElem))
            {
                message = messageElem.GetString() ?? message;
            }

            return new ApiResponse<string> { Success = status == 200, StatusCode = status, Message = message, Data = status == 200 ? email : null };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string> { Success = false, StatusCode = 500, Message = $"Connection error: {ex.Message}", Data = null };
        }
    }
    #endregion

    #region Sign In
    /// <summary>
    /// Sign in with email and password
    /// </summary>
    public static async Task<ApiResponse<UserData>> SignInAsync(string email, string password)
    {
        try
        {
            var url = $"{BaseUrl}/signin_action.php?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}";
            
            var response = await HttpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            
            using JsonDocument doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("status", out var statusElem) || !TryGetInt(statusElem, out int status))
            {
                return new ApiResponse<UserData> { Success = false, StatusCode = 500, Message = "Invalid response format: missing or invalid status", Data = null };
            }

            string message = "Unknown error";
            if (root.TryGetProperty("message", out var messageElem))
            {
                message = messageElem.GetString() ?? message;
            }

            if (status == 200 && root.TryGetProperty("data", out var dataElem) && dataElem.ValueKind == JsonValueKind.Object)
            {
                try
                {
                    var user = new UserData
                    {
                        Id = dataElem.TryGetProperty("id", out var idElem) ? idElem.GetInt32() : 0,
                        FirstName = dataElem.TryGetProperty("fname", out var fnameElem) ? fnameElem.GetString() ?? "" : "",
                        LastName = dataElem.TryGetProperty("lname", out var lnameElem) ? lnameElem.GetString() ?? "" : "",
                        Email = dataElem.TryGetProperty("email", out var emailElem) ? emailElem.GetString() ?? "" : "",
                        TimeModified = dataElem.TryGetProperty("timemodified", out var timeElem) ? timeElem.GetString() ?? "" : ""
                    };

                    return new ApiResponse<UserData> { Success = true, StatusCode = 200, Message = message, Data = user };
                }
                catch (Exception parseEx)
                {
                    return new ApiResponse<UserData> { Success = false, StatusCode = 500, Message = $"Error parsing user data: {parseEx.Message}", Data = null };
                }
            }

            return new ApiResponse<UserData> { Success = false, StatusCode = status, Message = message, Data = null };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UserData> { Success = false, StatusCode = 500, Message = $"Connection error: {ex.Message}", Data = null };
        }
    }
    #endregion

    #region Get ToDo Items
    /// <summary>
    /// Get active or inactive todo items for a user
    /// </summary>
    public static async Task<ApiResponse<List<ToDoItem>>> GetToDoItemsAsync(int userId, string status = "active")
    {
        try
        {
            var url = $"{BaseUrl}/getItems_action.php?status={Uri.EscapeDataString(status)}&user_id={userId}";
            
            var response = await HttpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            
            using JsonDocument doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("status", out var statusElem) || !TryGetInt(statusElem, out int statusCode))
            {
                return new ApiResponse<List<ToDoItem>> { Success = false, StatusCode = 500, Message = "Invalid response format: missing or invalid status", Data = new List<ToDoItem>() };
            }

            string message = "Unknown error";
            if (root.TryGetProperty("message", out var messageElem))
            {
                message = messageElem.GetString() ?? message;
            }

            if (statusCode == 200)
            {
                var items = new List<ToDoItem>();
                
                if (root.TryGetProperty("data", out var dataElem) && dataElem.ValueKind == JsonValueKind.Object)
                {
                    foreach (var item in dataElem.EnumerateObject())
                    {
                        var itemElem = item.Value;
                        try
                        {
                            items.Add(new ToDoItem
                            {
                                ItemId = itemElem.TryGetProperty("item_id", out var idElem) ? idElem.GetInt32() : 0,
                                ItemName = itemElem.TryGetProperty("item_name", out var nameElem) ? nameElem.GetString() ?? "" : "",
                                ItemDescription = itemElem.TryGetProperty("item_description", out var descElem) ? descElem.GetString() ?? "" : "",
                                Status = itemElem.TryGetProperty("status", out var statusField) ? statusField.GetString() ?? "active" : "active",
                                UserId = itemElem.TryGetProperty("user_id", out var userIdElem) ? userIdElem.GetInt32() : userId,
                                TimeModified = itemElem.TryGetProperty("timemodified", out var timeElem) ? timeElem.GetString() ?? "" : ""
                            });
                        }
                        catch
                        {
                            // Skip items that can't be parsed
                            continue;
                        }
                    }
                }

                return new ApiResponse<List<ToDoItem>> { Success = true, StatusCode = statusCode, Message = message, Data = items };
            }

            return new ApiResponse<List<ToDoItem>> { Success = false, StatusCode = statusCode, Message = message, Data = new List<ToDoItem>() };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<ToDoItem>> { Success = false, StatusCode = 500, Message = $"Connection error: {ex.Message}", Data = new List<ToDoItem>() };
        }
    }
    #endregion

    #region Add ToDo Item
    /// <summary>
    /// Add a new todo item
    /// </summary>
    public static async Task<ApiResponse<ToDoItem>> AddToDoItemAsync(string itemName, string itemDescription, int userId)
    {
        try
        {
            var payload = new { item_name = itemName, item_description = itemDescription, user_id = userId };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync($"{BaseUrl}/addItem_action.php", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;
            
            // Safely try to get status and message
            if (!root.TryGetProperty("status", out var statusElem) || !TryGetInt(statusElem, out int status))
            {
                return new ApiResponse<ToDoItem> { Success = false, StatusCode = 500, Message = "Invalid response format: missing or invalid status", Data = null };
            }

            string message = "Unknown error";
            if (root.TryGetProperty("message", out var messageElem))
            {
                message = messageElem.GetString() ?? message;
            }

            if (status == 200 && root.TryGetProperty("data", out var dataElem) && dataElem.ValueKind == JsonValueKind.Object)
            {
                try
                {
                    var item = new ToDoItem
                    {
                        ItemId = dataElem.TryGetProperty("item_id", out var idElem) ? idElem.GetInt32() : 0,
                        ItemName = dataElem.TryGetProperty("item_name", out var nameElem) ? nameElem.GetString() ?? "" : "",
                        ItemDescription = dataElem.TryGetProperty("item_description", out var descElem) ? descElem.GetString() ?? "" : "",
                        Status = dataElem.TryGetProperty("status", out var statusField) ? statusField.GetString() ?? "active" : "active",
                        UserId = dataElem.TryGetProperty("user_id", out var userIdElem) ? userIdElem.GetInt32() : userId,
                        TimeModified = dataElem.TryGetProperty("timemodified", out var timeElem) ? timeElem.GetString() ?? "" : ""
                    };
                    
                    return new ApiResponse<ToDoItem> { Success = true, StatusCode = 200, Message = message, Data = item };
                }
                catch (Exception parseEx)
                {
                    return new ApiResponse<ToDoItem> { Success = false, StatusCode = 500, Message = $"Error parsing response data: {parseEx.Message}", Data = null };
                }
            }

            return new ApiResponse<ToDoItem> { Success = false, StatusCode = status, Message = message, Data = null };
        }
        catch (Exception ex)
        {
            return new ApiResponse<ToDoItem> { Success = false, StatusCode = 500, Message = $"Connection error: {ex.Message}", Data = null };
        }
    }
    #endregion

    #region Update ToDo Item
    /// <summary>
    /// Update an existing todo item
    /// </summary>
    public static async Task<ApiResponse<string>> UpdateToDoItemAsync(int itemId, string itemName, string itemDescription)
    {
        try
        {
            var payload = new { item_id = itemId, item_name = itemName, item_description = itemDescription };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PUT"), $"{BaseUrl}/editItem_action.php") { Content = content };
            var response = await HttpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("status", out var statusElem) || !TryGetInt(statusElem, out int status))
            {
                return new ApiResponse<string> { Success = false, StatusCode = 500, Message = "Invalid response format: missing or invalid status", Data = null };
            }

            string message = "Unknown error";
            if (root.TryGetProperty("message", out var messageElem))
            {
                message = messageElem.GetString() ?? message;
            }

            return new ApiResponse<string> { Success = status == 200, StatusCode = status, Message = message, Data = status == 200 ? "Updated" : null };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string> { Success = false, StatusCode = 500, Message = $"Connection error: {ex.Message}", Data = null };
        }
    }
    #endregion

    #region Change ToDo Status
    /// <summary>
    /// Change todo item status (active/inactive)
    /// </summary>
    public static async Task<ApiResponse<string>> ChangeToDoStatusAsync(int itemId, string status)
    {
        try
        {
            var payload = new { status = status, item_id = itemId };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PUT"), $"{BaseUrl}/statusItem_action.php") { Content = content };
            var response = await HttpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("status", out var statusElem) || !TryGetInt(statusElem, out int statusCode))
            {
                return new ApiResponse<string> { Success = false, StatusCode = 500, Message = "Invalid response format: missing or invalid status", Data = null };
            }

            string message = "Unknown error";
            if (root.TryGetProperty("message", out var messageElem))
            {
                message = messageElem.GetString() ?? message;
            }

            return new ApiResponse<string> { Success = statusCode == 200, StatusCode = statusCode, Message = message, Data = statusCode == 200 ? status : null };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string> { Success = false, StatusCode = 500, Message = $"Connection error: {ex.Message}", Data = null };
        }
    }
    #endregion

    #region Delete ToDo Item
    /// <summary>
    /// Delete a todo item
    /// </summary>
    public static async Task<ApiResponse<string>> DeleteToDoItemAsync(int itemId)
    {
        try
        {
            var url = $"{BaseUrl}/deleteItem_action.php?item_id={itemId}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            var response = await HttpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;
            
            if (!root.TryGetProperty("status", out var statusElem) || !TryGetInt(statusElem, out int status))
            {
                return new ApiResponse<string> { Success = false, StatusCode = 500, Message = "Invalid response format: missing or invalid status", Data = null };
            }

            string message = "Unknown error";
            if (root.TryGetProperty("message", out var messageElem))
            {
                message = messageElem.GetString() ?? message;
            }

            return new ApiResponse<string> { Success = status == 200, StatusCode = status, Message = message, Data = status == 200 ? "Deleted" : null };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string> { Success = false, StatusCode = 500, Message = $"Connection error: {ex.Message}", Data = null };
        }
    }
    #endregion
}

#region API Response Models
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class UserData
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TimeModified { get; set; } = string.Empty;
}

public class ToDoItem
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string Status { get; set; } = "active";
    public int UserId { get; set; }
    public string TimeModified { get; set; } = string.Empty;
}
#endregion
