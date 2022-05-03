using GoodsKB.DAL.Repositories.Filters;
using UserRoles = GoodsKB.DAL.Entities.UserRoles;

namespace GoodsKB.API.Models;

public class UserModel
{
	[Filter] public int Id { get; set; }
	[Filter, GroupFilter("Name")] public string Username { get; set; } = string.Empty;
	[Filter, GroupFilter("Name")] public string? FirstName { get; set; }
	[Filter, GroupFilter("Name")] public string? LastName { get; set; }
	[Filter("Contacts")] public string? Email { get; set; }
	[Filter("Contacts")] public string? Phone { get; set; }
	public string? Desc { get; set; }
	public virtual IEnumerable<UserRoles> Roles { get; set; } = new List<UserRoles>();
	public virtual IEnumerable<DirectionModel> Directions {get; set; } = new List<DirectionModel>();
	[Filter, GroupFilter("Modified", 1, true)] public DateTimeOffset Created { get; set; }
	[Filter("Modified", 2, true)] public DateTimeOffset? Updated { get; set; }
	[Filter("Modified", 3, true)] public DateTimeOffset? Deleted { get; set; }
}