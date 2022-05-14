namespace GoodsKB.BLL.DTOs;

using GoodsKB.DAL.Entities;

public class Example1UpdateDto
{
	public Example1Id? Id { get; set; }

	public string? Name { get; set; }
	public int? Code { get; set; }
	public string? Desc { get; set; }

	public DateTimeOffset? Updated { get; set; }
}