using GoodsKB.DAL.Entities;

namespace GoodsKB.API.Models;

public class UserModel
{
	public int Id { get; set; }

	public string Username { get; set; } = string.Empty;
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? Phone { get; set; }
	public string? Desc { get; set; }

	public virtual IEnumerable<UserRoles> Roles {get; set; } = new List<UserRoles>();
	public virtual IEnumerable<DirectionModel> Directions {get; set; } = new List<DirectionModel>();

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}