namespace CarolCustomizer.Events;
public class UIFilterChangedEvent
{
    public readonly string FilterText;
    public readonly bool ShowFavorites;
    public readonly bool ShowActive;

    public UIFilterChangedEvent(string filterText, bool ShowFavorites, bool ShowActive)
    {
        this.FilterText = filterText;
        this.ShowFavorites = ShowFavorites;
        this.ShowActive = ShowActive;
    }
}
