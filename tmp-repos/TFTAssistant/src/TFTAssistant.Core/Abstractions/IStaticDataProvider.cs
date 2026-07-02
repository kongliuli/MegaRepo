using TFTAssistant.Core.Models.Static;

namespace TFTAssistant.Core.Abstractions;

public interface IStaticDataProvider
{
    Task<IReadOnlyList<string>> GetAvailableVersionsAsync();
    Task<string> GetLatestVersionAsync();
    Task<IReadOnlyList<Champion>> GetChampionsAsync(string set, string? version = null);
    Task<IReadOnlyList<Item>> GetItemsAsync(string set, string? version = null);
    Task<IReadOnlyList<Trait>> GetTraitsAsync(string set, string? version = null);
    Task<IReadOnlyList<AugmentData>> GetAugmentsAsync(string set, string? version = null);
    Task<IReadOnlyList<MetaComp>> GetMetaCompsAsync(string set, string? version = null);
    Task ClearVersionCacheAsync(string? version = null);
    Task<IReadOnlyList<string>> GetCachedVersionsAsync();
}
