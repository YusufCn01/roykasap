using CommunityToolkit.Mvvm.ComponentModel;
using KasapOtomasyon.Application.Interfaces;

namespace KasapOtomasyon.WPF.Services;

public class NavigationHistoryItem
{
    public ObservableObject ViewModel { get; set; } = null!;
    public string TitleKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> Breadcrumb { get; set; } = new();
}

public interface INavigationService
{
    ObservableObject CurrentViewModel { get; }
    string CurrentPageTitle { get; }
    List<string> Breadcrumb { get; }
    bool CanGoBack { get; }
    bool IsOnMainMenu { get; }

    event Action? CurrentPageChanged;

    void NavigateTo(ObservableObject viewModel, string pageTitleOrKey, params string[] breadcrumbs);
    void NavigateToMainMenu();
    void GoBack();
    void SetMainMenuViewModel(ObservableObject mainMenuVm);
}

public partial class NavigationService : ObservableObject, INavigationService
{
    private readonly ILocalizationService _locService;
    private readonly Stack<NavigationHistoryItem> _history = new();
    private ObservableObject? _mainMenuViewModel;
    private string _currentTitleKey = "shell.mainMenu";

    [ObservableProperty]
    private ObservableObject currentViewModel = null!;

    [ObservableProperty]
    private string currentPageTitle = "Ana Menü";

    [ObservableProperty]
    private List<string> breadcrumb = new() { "Ana Menü" };

    [ObservableProperty]
    private bool canGoBack;

    [ObservableProperty]
    private bool isOnMainMenu = true;

    public event Action? CurrentPageChanged;

    public NavigationService(ILocalizationService locService)
    {
        _locService = locService;
        _locService.LanguageChanged += (s, lang) => RefreshLocalizedTitles();
        RefreshLocalizedTitles();
    }

    private void RefreshLocalizedTitles()
    {
        var rootName = _locService.Get("shell.mainMenu");
        if (IsOnMainMenu || CurrentViewModel == _mainMenuViewModel)
        {
            CurrentPageTitle = rootName;
            Breadcrumb = new List<string> { rootName };
        }
        else
        {
            CurrentPageTitle = _locService.Get(_currentTitleKey);
            Breadcrumb = new List<string> { rootName, CurrentPageTitle };
        }
        CurrentPageChanged?.Invoke();
    }

    public void SetMainMenuViewModel(ObservableObject mainMenuVm)
    {
        _mainMenuViewModel = mainMenuVm;
        CurrentViewModel = mainMenuVm;
        _currentTitleKey = "shell.mainMenu";
        var rootName = _locService.Get("shell.mainMenu");
        CurrentPageTitle = rootName;
        Breadcrumb = new List<string> { rootName };
        CanGoBack = false;
        IsOnMainMenu = true;
        _history.Clear();
        CurrentPageChanged?.Invoke();
    }

    public void NavigateTo(ObservableObject viewModel, string pageTitleOrKey, params string[] breadcrumbs)
    {
        if (CurrentViewModel != null && CurrentViewModel != viewModel)
        {
            _history.Push(new NavigationHistoryItem
            {
                ViewModel = CurrentViewModel,
                TitleKey = _currentTitleKey,
                Title = CurrentPageTitle,
                Breadcrumb = new List<string>(Breadcrumb)
            });
        }

        _currentTitleKey = pageTitleOrKey;
        CurrentViewModel = viewModel;
        CurrentPageTitle = _locService.Get(pageTitleOrKey);

        var rootName = _locService.Get("shell.mainMenu");
        var fullBreadcrumbs = new List<string> { rootName };
        if (breadcrumbs != null && breadcrumbs.Length > 0)
        {
            fullBreadcrumbs.AddRange(breadcrumbs.Select(b => _locService.Get(b)));
        }
        else if (pageTitleOrKey != "shell.mainMenu" && pageTitleOrKey != "Ana Menü")
        {
            fullBreadcrumbs.Add(CurrentPageTitle);
        }

        Breadcrumb = fullBreadcrumbs;
        CanGoBack = _history.Count > 0;
        IsOnMainMenu = viewModel == _mainMenuViewModel;

        CurrentPageChanged?.Invoke();
    }

    public void NavigateToMainMenu()
    {
        if (_mainMenuViewModel != null)
        {
            _history.Clear();
            CurrentViewModel = _mainMenuViewModel;
            _currentTitleKey = "shell.mainMenu";
            var rootName = _locService.Get("shell.mainMenu");
            CurrentPageTitle = rootName;
            Breadcrumb = new List<string> { rootName };
            CanGoBack = false;
            IsOnMainMenu = true;
            CurrentPageChanged?.Invoke();
        }
    }

    public void GoBack()
    {
        if (_history.Count > 0)
        {
            var prev = _history.Pop();
            CurrentViewModel = prev.ViewModel;
            _currentTitleKey = prev.TitleKey;
            CurrentPageTitle = _locService.Get(prev.TitleKey);
            var rootName = _locService.Get("shell.mainMenu");
            Breadcrumb = new List<string> { rootName, CurrentPageTitle };
            CanGoBack = _history.Count > 0;
            IsOnMainMenu = CurrentViewModel == _mainMenuViewModel;
            CurrentPageChanged?.Invoke();
        }
        else
        {
            NavigateToMainMenu();
        }
    }
}
