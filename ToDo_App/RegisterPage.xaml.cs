namespace ToDo_App;
public partial class RegisterPage : ContentPage
{
    public RegisterPage() { InitializeComponent(); }

    private async void OnRegisterButtonClicked(object sender, EventArgs e)
    {
        string firstName = UserRegisterInput.Text?.Trim() ?? string.Empty;
        string lastName  = string.Empty; // Optional - API can accept empty
        string email     = EmailRegisterInput.Text?.Trim() ?? string.Empty;
        string password  = PassRegisterInput.Text?.Trim() ?? string.Empty;
        string confirm   = ConfirmPassInput.Text?.Trim() ?? string.Empty;

        // Validate all fields are filled
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(email) || 
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm))
        {
            await DisplayAlertAsync("Error", "Please fill in all fields.", "OK");
            return;
        }

        // Validate passwords match
        if (password != confirm)
        {
            await DisplayAlertAsync("Error", "Passwords do not match.", "OK");
            return;
        }

        // Call Sign Up API
        var result = await ApiService.SignUpAsync(firstName, lastName, email, password, confirm);

        if (!result.Success)
        {
            await DisplayAlertAsync("Error", result.Message, "OK");
            return;
        }

        await DisplayAlertAsync("Success", "Account created! You can now sign in.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnLoginLabelTapped(object sender, TappedEventArgs e)
        => await Navigation.PopAsync();
}