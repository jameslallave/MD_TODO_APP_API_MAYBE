namespace ToDo_App;
public partial class LoginPage : ContentPage
{
    public LoginPage() { InitializeComponent(); }

    private async void OnLoginButtonClicked(object sender, EventArgs e)
    {
        string email    = EmailInput.Text?.Trim() ?? string.Empty;
        string password = PassInput.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Error", "Please enter your Email and Password.", "OK");
            return;
        }

        // Show loading indicator (optional)
        var result = await ApiService.SignInAsync(email, password);

        if (!result.Success)
        {
            await DisplayAlertAsync("Error", result.Message, "OK");
            return;
        }

        // Store user information
        if (result.Data != null)
        {
            AppService.UserId = result.Data.Id;
            AppService.FirstName = result.Data.FirstName;
            AppService.LastName = result.Data.LastName;
            AppService.Email = result.Data.Email;
            AppService.Password = password;
        }

        // Load active todos
        var activeResult = await ApiService.GetToDoItemsAsync(AppService.UserId, "active");
        if (activeResult.Success && activeResult.Data != null)
        {
            AppService.TodoItems.Clear();
            foreach (var item in activeResult.Data)
            {
                AppService.TodoItems.Add(new ToDoClass
                {
                    Id = item.ItemId,
                    Title = item.ItemName,
                    Detail = item.ItemDescription,
                    Status = item.Status,
                    UserId = item.UserId,
                    TimeModified = item.TimeModified
                });
            }
        }

        // Load inactive (completed) todos
        var inactiveResult = await ApiService.GetToDoItemsAsync(AppService.UserId, "inactive");
        if (inactiveResult.Success && inactiveResult.Data != null)
        {
            AppService.CompletedItems.Clear();
            foreach (var item in inactiveResult.Data)
            {
                AppService.CompletedItems.Add(new ToDoClass
                {
                    Id = item.ItemId,
                    Title = item.ItemName,
                    Detail = item.ItemDescription,
                    Status = item.Status,
                    UserId = item.UserId,
                    TimeModified = item.TimeModified
                });
            }
        }

        await Shell.Current.GoToAsync("//MainTabs");
    }

    private async void OnRegisterLabelTapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new RegisterPage());
}