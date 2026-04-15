namespace ToDo_App;

public partial class ProfilePage : ContentPage
{
    public ProfilePage() { InitializeComponent(); }

    public void NotifyAppearing() => RefreshData();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshData();
    }

    private void RefreshData()
    {
        // Display user's full name from API
        string displayName = string.Empty;
        if (!string.IsNullOrEmpty(AppService.FirstName) || !string.IsNullOrEmpty(AppService.LastName))
        {
            displayName = $"{AppService.FirstName} {AppService.LastName}".Trim();
        }
        else if (!string.IsNullOrEmpty(AppService.Username))
        {
            displayName = AppService.Username;
        }
        else
        {
            displayName = "Guest";
        }

        UsernameLabel.Text = displayName;
        EmailLabel.Text    = string.IsNullOrEmpty(AppService.Email) ? "No email set" : AppService.Email;
        PendingCount.Text  = AppService.TodoItems.Count.ToString();
        DoneCount.Text     = AppService.CompletedItems.Count.ToString();
    }

    private async void OnSignOutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync("Sign Out", "Are you sure you want to sign out?", "Yes", "No");
        if (!confirm) return;
        
        // Clear user data
        AppService.ClearUserData();
        
        await Shell.Current.GoToAsync("//LoginPage");
    }
}