namespace ToDo_App;

public partial class CompletedPage : ContentPage
{
    public CompletedPage()
    {
        InitializeComponent();
        completedLV.ItemsSource = AppService.CompletedItems;
        AppService.CompletedItems.CollectionChanged += (_, _) => UpdateEmptyState();
        UpdateEmptyState();
    }

    public void NotifyAppearing() => UpdateEmptyState();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateEmptyState();
    }

    private async void DeleteCompletedItem(object sender, EventArgs e)
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

        AppService.CompletedItems.Remove(item);
        UpdateEmptyState();
    }

    private async void completedLV_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ToDoClass selected)
            await Shell.Current.Navigation.PushAsync(new EditPage(selected, isFromCompleted: true));
    }

    private void UpdateEmptyState()
        => EmptyLabel.IsVisible = AppService.CompletedItems.Count == 0;
}