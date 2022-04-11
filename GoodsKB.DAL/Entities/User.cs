using  GoodsKB.DAL.Repositories;

namespace GoodsKB.DAL.Entities;

public class User : IEntityId<int>
{
	public int Id { get; set; }
	public void SetId(int id) => Id = id;
	public int GetId() => Id;

	public string? Username { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? Phone { get; set; }
	public string? Desc { get; set; }

	public string? UsernameHash { get; set; }
	public string? EmailHash { get; set; }
	public string? PhoneHash { get; set; }

	public virtual IEnumerable<UserRoles> Roles {get; set; } = new List<UserRoles>();
	public virtual IEnumerable<Direction> Directions {get; set; } = new List<Direction>();

	public DateTimeOffset Created { get; set; }
	public DateTimeOffset Updated { get; set; }
	public DateTimeOffset Deleted { get; set; }
}