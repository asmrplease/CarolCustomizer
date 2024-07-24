namespace CarolCustomizer.Events;
public class UIFilterChangedEvent
{
    public readonly string Text;
    public readonly bool ShowFavorites;
    public readonly bool ShowActive;
    public readonly bool AnyFilters;
    public readonly bool HasText;

    public UIFilterChangedEvent(string filterText, bool ShowFavorites, bool ShowActive)
    {
        this.Text = filterText.Trim();
        this.ShowFavorites = ShowFavorites;
        this.ShowActive = ShowActive;
        this.HasText = this.Text != string.Empty;
        this.AnyFilters = HasText
            || ShowActive
            || ShowFavorites;
    }

    public override string ToString()
    {
        return 
            $"Enabled: {AnyFilters}, " +
            $"HasText: {HasText}, " +
            $"Text: '{Text}', " +
            $"ShowFavorites: {ShowFavorites}, " +
            $"ShowActive: {ShowActive}.";
    }
}
