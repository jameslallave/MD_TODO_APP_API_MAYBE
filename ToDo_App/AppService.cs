using System.Collections.ObjectModel;

namespace ToDo_App;

public static class AppService
{
    // User Information
    public static int UserId        { get; set; } = 0;
    public static string FirstName  { get; set; } = string.Empty;
    public static string LastName   { get; set; } = string.Empty;
    public static string Email      { get; set; } = string.Empty;
    public static string Password   { get; set; } = string.Empty;
    public static string Username   { get; set; } = string.Empty;

    // Todo Collections
    public static ObservableCollection<ToDoClass> TodoItems      { get; } = new();
    public static ObservableCollection<ToDoClass> CompletedItems { get; } = new();

    // Clear user data on logout
    public static void ClearUserData()
    {
        UserId = 0;
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        Username = string.Empty;
        TodoItems.Clear();
        CompletedItems.Clear();
    }
}