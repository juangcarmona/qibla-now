using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

/// <summary>
/// No-op Adhan player used on non-Android platforms.
/// </summary>
public sealed class NullAdhanPlayer : IAdhanPlayer
{
    public void Preview(AdhanSound sound) { }
    public void StopPreview() { }
}
