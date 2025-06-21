namespace CarolCustomizer.Events;
public class HairChangeEvent(string style, string color)
{
    public readonly string Style = style;
    public readonly string Color = color;
}
