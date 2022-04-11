using GoodsKB.DAL.Entities;

namespace GoodsKB.BLL.DTOs;

public class UserUpdateDto
{
	public int Id { get; set; }

	public string? Username { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? Phone { get; set; }
	public string? Desc { get; set; }

	public virtual IEnumerable<UserRoles> Roles {get; set; } = new List<UserRoles>();
	//public virtual IEnumerable<Direction> Directions {get; set; } = new List<Direction>();

	public DateTimeOffset Updated { get; set; }
}