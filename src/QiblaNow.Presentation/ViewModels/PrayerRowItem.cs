namespace QiblaNow.Presentation.ViewModels;

/// <summary>
/// Display model for a single prayer time row on the Home screen.
/// </summary>
public sealed class PrayerRowItem
{
    public string Name { get; }
    public string Time { get; }
    public bool IsHighlighted { get; }

    public PrayerRowItem(string name, string time, bool isHighlighted = false)
    {
        Name = name;
        Time = time;
        IsHighlighted = isHighlighted;
    }
}