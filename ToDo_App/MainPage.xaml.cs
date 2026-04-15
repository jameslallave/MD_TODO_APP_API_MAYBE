namespace ToDo_App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        todoLV.ItemsSource = AppService.TodoItems;
        AppService.TodoItems.CollectionChanged += (_, _) => UpdateEmptyState();
        UpdateEmptyState();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateEmptyState();
    }

    private async void todoLV_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ToDoClass selected)
            await Shell.Current.Navigation.PushAsync(new EditPage(selected, isFromCompleted: false));
    }

    private async void QuickCompleteItem(object sender, EventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.ClassId, out int id))
        {
            var item = AppService.TodoItems.FirstOrDefault(t => t.Id == id);
            if (item != null)
            {
                // Change status to inactive on API
                var result = await ApiService.ChangeToDoStatusAsync(item.Id, "inactive");
                if (result.Success)
                {
                    AppService.TodoItems.Remove(item);
                    item.Status = "inactive";
                    AppService.CompletedItems.Add(item);
                }
                else
                {
                    await DisplayAlertAsync("Error", result.Message, "OK");
                }
            }
        }
        UpdateEmptyState();
    }

    private async void DeleteToDoItem(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        
        var item = btn.BindingContext as ToDoClass;
        if (item == null) return;
        
        bool confirm = await DisplayAlertAsync("Delete", "Delete this task?", "Yes", "No");
        if (!confirm) return;

        // Call Delete API
        var result = await ApiService.DeleteToDoItemAsync(item.Id);
        if (!result.Success)
        {
            await DisplayAlertAsync("Error", result.Message, "OK");
            return;
        }

        AppService.TodoItems.Remove(item);
        UpdateEmptyState();
    }

    private void UpdateEmptyState()
        => EmptyLabel.IsVisible = AppService.TodoItems.Count == 0;

    private async void OpenAddPage(object sender, EventArgs e)
        => await Shell.Current.Navigation.PushAsync(new AddPage());
}