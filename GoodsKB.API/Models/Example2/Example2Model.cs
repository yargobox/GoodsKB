namespace GoodsKB.API.Models;

using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Filters;
using GoodsKB.DAL.Repositories.SortOrders;

public class Example2Model
{
	public Example2Id? Id { get; set; }

	public Example2Id? ForeignKey { get; set; }

	public string? Desc { get; set; }
	
	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}