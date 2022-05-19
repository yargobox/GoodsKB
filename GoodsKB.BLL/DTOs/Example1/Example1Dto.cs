namespace GoodsKB.BLL.DTOs;

using GoodsKB.DAL.Entities;

public class Example1Dto
{
	public Example1Id? Id { get; set; }

	public string? Desc { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}