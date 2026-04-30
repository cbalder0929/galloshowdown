namespace GalloShowdown.Models;

public class Stable
{
    private readonly List<Rooster> _roosters;
    private int _viewIndex;
    private int _selectedIndex;

    public Stable(IEnumerable<Rooster> initial)
    {
        _roosters = initial.ToList();
        if (_roosters.Count == 0)
            throw new ArgumentException("Stable needs at least one rooster.");
    }

    public Rooster Current  => _roosters[_viewIndex];
    public Rooster Selected => _roosters[_selectedIndex];
    public int     Count    => _roosters.Count;
    public bool    CurrentIsSelected => _viewIndex == _selectedIndex;

    public void Next()          => _viewIndex = (_viewIndex + 1) % _roosters.Count;
    public void Prev()          => _viewIndex = (_viewIndex - 1 + _roosters.Count) % _roosters.Count;
    public void SelectCurrent() => _selectedIndex = _viewIndex;
}
