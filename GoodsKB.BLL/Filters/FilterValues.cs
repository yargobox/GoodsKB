using System.Collections.ObjectModel;

namespace GoodsKB.BLL.Services;

public sealed record FilterValues(ReadOnlyDictionary<string, FilterDefinition> Definitions, IEnumerable<FilterValue> Values)
{
}