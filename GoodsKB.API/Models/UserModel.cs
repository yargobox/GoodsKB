using GoodsKB.DAL.Repositories.Filters;
using UserRoles = GoodsKB.DAL.Entities.UserRoles;

namespace GoodsKB.API.Models;

public class UserModel
{
	[Filter(Position = 1)]
	public int Id { get; set; }

	[Filter(Visible = false), GroupFilter("Name", Position = 3, ConditionOrder = 1)]
	public string Username { get; set; } = string.Empty;
	
	[Filter(Visible = false), GroupFilter("Name", ConditionOrder = 3)]
	public string? FirstName { get; set; }
	
	[Filter(Visible = false), GroupFilter("Name", ConditionOrder = 2)]
	public string? LastName { get; set; }
	
	[Filter("Contacts", Position = 4, ConditionOrder = 2)]
	public string? Email { get; set; }
	
	[Filter("Contacts", ConditionOrder = 1, Visible = true)]
	public string? Phone { get; set; }
	
	public string? Desc { get; set; }
	
	public virtual IEnumerable<UserRoles> Roles { get; set; } = new List<UserRoles>();
	
	public virtual IEnumerable<DirectionModel> Directions {get; set; } = new List<DirectionModel>();
	
	[Filter(Position = 2), GroupFilter("Modified", 5, true, ConditionOrder = 1)]
	public DateTimeOffset Created { get; set; }
	
	[Filter("Modified")]
	public DateTimeOffset? Updated { get; set; }
	
	[Filter("Modified")]
	public DateTimeOffset? Deleted { get; set; }
}