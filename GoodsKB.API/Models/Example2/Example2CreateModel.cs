namespace GoodsKB.API.Models;

using GoodsKB.DAL.Entities;

public class Example2CreateModel
{
	public Example2Id? Id { get; set; }

	public Example2Id? ForeignKey { get; set; }
	
	public string? Desc { get; set; }
}