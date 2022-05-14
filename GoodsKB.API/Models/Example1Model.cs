namespace GoodsKB.API.Models;

using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Filters;
using GoodsKB.DAL.Repositories.SortOrders;

public class Example1Model
{
	public Example1Id? Id { get; set; }

	public string? Name { get; set; }

	public int? Code { get; set; }

	public string? Desc { get; set; }
	
	public DateTimeOffset? Created { get; set; }

	public DateTimeOffset? Updated { get; set; }
	
	public DateTimeOffset? Deleted { get; set; }
}