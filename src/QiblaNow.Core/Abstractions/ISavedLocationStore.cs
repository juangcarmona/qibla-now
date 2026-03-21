using QiblaNow.Core.Models;

namespace QiblaNow.Core.Abstractions;

public interface ISavedLocationStore
{
    IReadOnlyList<SavedLocation> GetRecentLocations();
    void UpsertRecentLocation(SavedLocation location);
}
