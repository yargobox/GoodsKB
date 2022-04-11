namespace GoodsKB.BLL.DTOs;

public class DirectionDto
{
	public int Id { get; set; }

	public string? Name { get; set; }
	public string? Desc { get; set; }

	//public virtual IEnumerable<ProdCat1> ProdCats1 { get; set; } = new List<ProdCat1>();

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset Updated { get; set; }
	public DateTimeOffset Deleted { get; set; }
}