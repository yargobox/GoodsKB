namespace GoodsKB.BLL.DTOs;

using GoodsKB.DAL.Entities;

public class Example2Dto
{
	public Example2Id? Id { get; set; }

	public Example2Id? ForeignKey { get; set; }

	public string? Desc { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}