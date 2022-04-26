using GoodsKB.BLL.Services;
using GoodsKB.DAL.Entities;
using FilterOperations = GoodsKB.DAL.Repositories.FilterOperations;

namespace GoodsKB.BLL.DTOs;

public class UserDto
{
	public int Id { get; set; }
	public string Username { get; set; } = string.Empty;
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Email { get; set; }
	public string? Phone { get; set; }
	public string? Desc { get; set; }
	public UserRoles? Roles { get; set; }
	public virtual IEnumerable<DirectionDto> Directions {get; set; } = new List<DirectionDto>();
	public DateTimeOffset Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}