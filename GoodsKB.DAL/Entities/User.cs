using  GoodsKB.DAL.Repositories;
using MongoDB.Bson.Serialization.Attributes;

namespace GoodsKB.DAL.Entities;

public class User : IUpdatedEntity<int?, DateTimeOffset>, ISoftDelEntity<DateTimeOffset>
{
	public int? Id { get; set; }

	public string? Username { get; set; }
	public string? FirstName { get; set; }
	public string? LastName { get; set; }

	[BsonIgnoreIfNull]
	public string? Email { get; set; }

	[BsonIgnoreIfNull]
	public string? Phone { get; set; }
	
	public string? Desc { get; set; }

	public string? UsernameHash { get; set; }
	public string? EmailHash { get; set; }
	public string? PhoneHash { get; set; }

	public virtual IEnumerable<UserRoles> Roles { get; set; } = new List<UserRoles>();
	public virtual IEnumerable<Direction> Directions {get; set; } = new List<Direction>();

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}