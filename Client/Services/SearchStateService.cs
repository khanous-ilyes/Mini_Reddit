using System;
using BaseLibrary.DTOs.Posts;

namespace Client.Services;

public class SearchStateService
{
    private SearchFilterDto? _currentFilter;

    public SearchFilterDto? CurrentFilter => _currentFilter;
    public bool IsActive => _currentFilter != null;

    public event Action? OnFilterChanged;

    public void ApplyFilter(SearchFilterDto filter)
    {
        _currentFilter = filter;
        NotifyStateChanged();
    }

    public void ClearFilter()
    {
        _currentFilter = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnFilterChanged?.Invoke();
}
