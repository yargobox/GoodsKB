namespace GoodsKB.BLL.DTOs;

using GoodsKB.DAL.Entities;

public class Example2CreateDto
{
	public Example2Id? Id { get; set; }

	public Example2Id? ForeignKey { get; set; }

	public string? Desc { get; set; }
}