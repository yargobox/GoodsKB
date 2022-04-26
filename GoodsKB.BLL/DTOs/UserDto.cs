using GoodsKB.BLL.Services;
using GoodsKB.DAL.Entities;
using FilterOperations = GoodsKB.DAL.Repositories.FilterOperations;

namespace GoodsKB.BLL.DTOs;

public class UserDto
{
	[UserFilter(false, false, FilterOperations.Equal, FO.Equality | FO.Arithmetic | FO.Inclusion | FO.Bitwise)]
	public int Id { get; set; }

	[UserFilter]//[UserFilter(FilterOperations.Like | FilterOperations.CaseInsensitive, FilterOperations.Equal | FilterOperations.NotEqual | FilterOperations.In | FilterOperations.NotIn | FilterOperations.Like | FilterOperations.NotLike | FilterOperations.CaseInsensitive)]
	public string Username { get; set; } = string.Empty;
	
	[UserFilter(true, true, FilterOperations.Like | FilterOperations.CaseInsensitive)]
	public string? FirstName { get; set; }

	[UserFilter]
	public string? LastName { get; set; }

	[UserFilter]
	public string? Email { get; set; }

	[UserFilter]
	public string? Phone { get; set; }

	[UserFilter]
	public string? Desc { get; set; }

	[UserFilter]
	public UserRoles? Roles { get; set; }
	public virtual IEnumerable<DirectionDto> Directions {get; set; } = new List<DirectionDto>();

	[UserFilter]
	public DateTimeOffset Created { get; set; }

	[UserFilter]
	public DateTimeOffset? Updated { get; set; }

	[UserFilter]
	public DateTimeOffset? Deleted { get; set; }
}