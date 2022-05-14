namespace GoodsKB.BLL.DTOs;

public class BrandUpdateDto
{
	public int? Id { get; set; }

	public string? Name { get; set; }

	public string? Desc { get; set; }

	public DateTimeOffset? Updated { get; set; }
}