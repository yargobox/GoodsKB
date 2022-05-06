using GoodsKB.DAL.Repositories.Filters;
using UserRoles = GoodsKB.DAL.Entities.UserRoles;

namespace GoodsKB.API.Models;

public class UserModel
{
	[Filter(Position = 1)]
	public int Id { get; set; }

	[Filter(Visible = false)]
	[GroupFilter("Name", Position = 3)]
	public string Username { get; set; } = string.Empty;
	
	[FilterPart("Name")]
	public string? FirstName { get; set; }
	
	[FilterPart("Name", ConditionOrder = 2)]
	public string? LastName { get; set; }
	
	[FilterPart("Contacts", ConditionOrder = 1)]
	public string? Email { get; set; }
	
	[GroupFilter("Contacts", Position = 4)]
	public string? Phone { get; set; }
	
	public string? Desc { get; set; }
	
	public virtual IEnumerable<UserRoles> Roles { get; set; } = new List<UserRoles>();
	
	public virtual IEnumerable<DirectionModel> Directions {get; set; } = new List<DirectionModel>();
	
	[GroupFilter("Modified", Position = 1, ConditionOrder = 2)]
	[Filter(Position = 2)]
	public DateTimeOffset Created { get; set; }
	
	[FilterPart("Modified", 3)]
	public DateTimeOffset? Updated { get; set; }
	
	[FilterPart("Modified", 1)]
	public DateTimeOffset? Deleted { get; set; }
}