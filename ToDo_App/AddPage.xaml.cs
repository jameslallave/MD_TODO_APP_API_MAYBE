namespace ToDo_App;
public partial class AddPage : ContentPage
{
    public AddPage() { InitializeComponent(); }

    private async void AddToDoItem(object sender, EventArgs e)
    {
        string title = TitleEntry.Text?.Trim() ?? string.Empty;
        string detail = DetailsEditor.Text?.Trim() ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Error", "Please enter a task title.", "OK");
            return;
        }

        // Call Add To Do API
        var result = await ApiService.AddToDoItemAsync(title, detail, AppService.UserId);

        if (!result.Success)
        {
            await DisplayAlertAsync("Error", result.Message, "OK");
            return;
        }

        // Add to local collection
        if (result.Data != null)
        {
            AppService.TodoItems.Add(new ToDoClass
            {
                Id = result.Data.ItemId,
                Title = result.Data.ItemName,
                Detail = result.Data.ItemDescription,
                Status = result.Data.Status,
                UserId = result.Data.UserId,
                TimeModified = result.Data.TimeModified
            });
        }

        await Navigation.PopAsync();
    }
}