using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace GoodsKB.BLL.Services;

public interface ISortOrderWork<TEntity>
{
	ReadOnlyCollection<IFieldSortOrderDefinition> Definitions { get; }

	IEnumerable<FieldSortOrderItem> SerializeFromString(string s);
	string SerializeToString(IEnumerable<FieldSortOrderItem> items);
	Expression BuildConditionPredicate(IEnumerable<IEnumerable<FieldSortOrderItem>> items);
	Expression<Func<TEntity, bool>> BuildCondition(IEnumerable<IEnumerable<FieldSortOrderItem>> items);
}