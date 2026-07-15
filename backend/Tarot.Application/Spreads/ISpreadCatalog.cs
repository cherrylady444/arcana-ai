using Tarot.Domain;

namespace Tarot.Application.Spreads;

public interface ISpreadCatalog
{
    IReadOnlyList<SpreadDefinition> GetBuiltInSpreads();
    SpreadDefinition? GetByKey(string key);
}
