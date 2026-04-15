namespace ToDo_App;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ToDoClass : INotifyPropertyChanged
{
    public ToDoClass() { }

    private int    _id;
    private string _title       = string.Empty;
    private string _detail      = string.Empty;
    private string _status      = "active";
    private int    _userId      = 0;
    private string _timeModified = string.Empty;

    public int Id
    {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }
    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }
    public string Detail
    {
        get => _detail;
        set { _detail = value; OnPropertyChanged(); }
    }
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }
    public int UserId
    {
        get => _userId;
        set { _userId = value; OnPropertyChanged(); }
    }
    public string TimeModified
    {
        get => _timeModified;
        set { _timeModified = value; OnPropertyChanged(); }
    }

    // Helper property for UI
    public bool IsCompleted
    {
        get => _status == "inactive";
        set { Status = value ? "inactive" : "active"; }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}