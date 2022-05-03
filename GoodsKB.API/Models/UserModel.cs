using GoodsKB.DAL.Repositories.Filters;
using UserRoles = GoodsKB.DAL.Entities.UserRoles;

namespace GoodsKB.API.Models;

public class UserModel
{
	[Filter] public int Id { get; set; }
	[Filter] public string Username { get; set; } = string.Empty;
	[Filter] public string? FirstName { get; set; }
	[Filter] public string? LastName { get; set; }
	[Filter] public string? Email { get; set; }
	[Filter] public string? Phone { get; set; }
	public string? Desc { get; set; }
	public virtual IEnumerable<UserRoles> Roles { get; set; } = new List<UserRoles>();
	public virtual IEnumerable<DirectionModel> Directions {get; set; } = new List<DirectionModel>();
	[Filter] public DateTimeOffset Created { get; set; }
	[Filter] public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}