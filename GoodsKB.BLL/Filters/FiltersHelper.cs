using System.Collections.ObjectModel;
using System.Linq.Expressions;
using GoodsKB.DAL.Repositories;

namespace GoodsKB.BLL.Services;

public static class FiltersHelper<TEntity>
	where TEntity : class
{
	public static ReadOnlyCollection<FieldFilterDefinition> Definitions { get; }

	public static IEnumerable<FieldFilterItem> SerializeFromString(string s)
	{

	}
	public static string SerializeToString(IEnumerable<FieldFilterItem> items)
	{

	}
	public static Expression BuildConditionPredicate(IEnumerable<FieldFilterItem> items)
	{
		foreach (var item in items)
		{
			var info = FieldPredicate<TEntity>.GetOperandsInfo(item.Operation);
			var predicate = info.operandCount switch
				{
					1 => FieldPredicate<TEntity>.Build(item.Operation, );
				}
		}
	}
	public static Expression<Func<TEntity, bool>> BuildCondition(IEnumerable<FieldFilterItem> items)
	{
		return Expression.Lambda<Func<TEntity, bool>>(BuildConditionPredicate(items));
	}
}