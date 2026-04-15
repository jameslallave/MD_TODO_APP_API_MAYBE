namespace ToDo_App;

public partial class EditPage : ContentPage
{
    private readonly ToDoClass _item;
    private readonly bool      _isFromCompleted;

    public EditPage(ToDoClass item, bool isFromCompleted)
    {
        InitializeComponent();
        _item            = item;
        _isFromCompleted = isFromCompleted;

        TitleEntry.Text    = item.Title;
        DetailsEditor.Text = item.Detail;

        // Swap button label/color depending on which list we came from
        if (_isFromCompleted)
        {
            CompleteBtn.Text            = "Incomplete";
            CompleteBtn.BackgroundColor = Colors.Gray;
        }
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        string title = TitleEntry.Text?.Trim() ?? string.Empty;
        string detail = DetailsEditor.Text?.Trim() ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Error", "Title cannot be empty.", "OK");
            return;
        }

        // Call Update API
        var result = await ApiService.UpdateToDoItemAsync(_item.Id, title, detail);

        if (!result.Success)
        {
            await DisplayAlertAsync("Error", result.Message, "OK");
            return;
        }

        _item.Title  = title;
        _item.Detail = detail;
        await Navigation.PopAsync();
    }

    private async void OnCompleteToggleClicked(object sender, EventArgs e)
    {
        if (!_isFromCompleted)
        {
            // Move from Todo → Completed
            string title = TitleEntry.Text?.Trim() ?? _item.Title;
            string detail = DetailsEditor.Text?.Trim() ?? _item.Detail;

            // Update on API first
            var updateResult = await ApiService.UpdateToDoItemAsync(_item.Id, title, detail);
            if (!updateResult.Success)
            {
                await DisplayAlertAsync("Error", updateResult.Message, "OK");
                return;
            }

            // Change status to inactive
            var statusResult = await ApiService.ChangeToDoStatusAsync(_item.Id, "inactive");
            if (!statusResult.Success)
            {
                await DisplayAlertAsync("Error", statusResult.Message, "OK");
                return;
            }

            AppService.TodoItems.Remove(_item);
            _item.Status = "inactive";
            AppService.CompletedItems.Add(_item);
        }
        else
        {
            // Move from Completed → Todo (change status to active)
            var statusResult = await ApiService.ChangeToDoStatusAsync(_item.Id, "active");
            if (!statusResult.Success)
            {
                await DisplayAlertAsync("Error", statusResult.Message, "OK");
                return;
            }

            AppService.CompletedItems.Remove(_item);
            _item.Status = "active";
            AppService.TodoItems.Add(_item);
        }

        await Navigation.PopAsync();
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync("Delete", "Delete this task?", "Yes", "No");
        if (!confirm) return;

        // Call Delete API
        var result = await ApiService.DeleteToDoItemAsync(_item.Id);

        if (!result.Success)
        {
            await DisplayAlertAsync("Error", result.Message, "OK");
            return;
        }

        if (_isFromCompleted)
            AppService.CompletedItems.Remove(_item);
        else
            AppService.TodoItems.Remove(_item);

        await Navigation.PopAsync();
    }
}