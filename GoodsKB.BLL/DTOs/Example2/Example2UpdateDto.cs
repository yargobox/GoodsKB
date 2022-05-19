namespace GoodsKB.BLL.DTOs;

using GoodsKB.DAL.Entities;

public class Example2UpdateDto
{
	public Example2Id? ForeignKey { get; set; }
	
	public string? Desc { get; set; }
}