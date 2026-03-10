namespace QiblaNow.Core.Abstractions;

/// <summary>
/// Abstracts the system clock so all "now" references can be injected and tested deterministically.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
