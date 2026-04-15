namespace ToDo_App;

public partial class MainTabsPage : ContentPage
{
    private readonly MainPage      _todoPage      = new();
    private readonly CompletedPage _completedPage = new();
    private readonly ProfilePage   _profilePage   = new();

    private int _currentTab = -1;

    public MainTabsPage()
    {
        InitializeComponent();
        ShowTab(0);
    }

    private void OnTodoTabTapped(object sender, TappedEventArgs e)      => ShowTab(0);
    private void OnCompletedTabTapped(object sender, TappedEventArgs e) => ShowTab(1);
    private void OnProfileTabTapped(object sender, TappedEventArgs e)   => ShowTab(2);

    private void ShowTab(int index)
    {
        if (_currentTab == index) return;
        _currentTab = index;

        TabContent.Content = index switch
        {
            0 => _todoPage.Content,
            1 => _completedPage.Content,
            2 => _profilePage.Content,
            _ => _todoPage.Content
        };

        if (index == 1) _completedPage.NotifyAppearing();
        if (index == 2) _profilePage.NotifyAppearing();

        LabelTodo.TextColor      = index == 0 ? Color.FromArgb("#ccc2ff") : Color.FromArgb("#666");
        LabelCompleted.TextColor = index == 1 ? Color.FromArgb("#ccc2ff") : Color.FromArgb("#666");
        LabelProfile.TextColor   = index == 2 ? Color.FromArgb("#ccc2ff") : Color.FromArgb("#666");
        IconTodo.TextColor       = index == 0 ? Color.FromArgb("#ccc2ff") : Color.FromArgb("#666");
        IconCompleted.TextColor  = index == 1 ? Color.FromArgb("#ccc2ff") : Color.FromArgb("#666");
        IconProfile.TextColor    = index == 2 ? Color.FromArgb("#ccc2ff") : Color.FromArgb("#666");
    }
}