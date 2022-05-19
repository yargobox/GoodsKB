namespace GoodsKB.API.Models;

using GoodsKB.DAL.Entities;

public class Example2UpdateModel
{
	public Example2Id? ForeignKey { get; set; }
	
	public string? Desc { get; set; }
}