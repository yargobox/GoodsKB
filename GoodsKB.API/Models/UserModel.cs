using GoodsKB.BLL.Services;
using UserRoles = GoodsKB.DAL.Entities.UserRoles;

namespace GoodsKB.API.Models;

public class UserModel
{
	[UserFilter] public int Id { get; set; }
	[UserFilter] public string Username { get; set; } = string.Empty;
	[UserFilter] public string? FirstName { get; set; }
	[UserFilter] public string? LastName { get; set; }
	[UserFilter] public string? Email { get; set; }
	[UserFilter] public string? Phone { get; set; }
	public string? Desc { get; set; }
	public virtual IEnumerable<UserRoles> Roles { get; set; } = new List<UserRoles>();
	public virtual IEnumerable<DirectionModel> Directions {get; set; } = new List<DirectionModel>();
	[UserFilter] public DateTimeOffset Created { get; set; }
	[UserFilter] public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}