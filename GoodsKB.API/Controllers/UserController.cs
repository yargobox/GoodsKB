using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GoodsKB.API.Models;
using GoodsKB.BLL.Common;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using SoftDelModes = GoodsKB.DAL.Repositories.SoftDelModes;
using User = GoodsKB.DAL.Entities.User;

namespace GoodsKB.API.Controllers;

[ApiController]
[Route("api/[controller]s")]
[Produces("application/json")]
public class UserController : ControllerBase
{
	private readonly IUserService _userService;
	private readonly IMapper _mapper;
	private readonly ILogger<UserController> _logger;

	public UserController(IUserService userService, IMapper mapper, ILogger<UserController> logger)
	{
		_userService = userService;
		_mapper = mapper;
		_logger = logger;
	}

	[HttpGet]
	[ProducesResponseType(typeof(PagedResponse<IEnumerable<UserModel>>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAsync(
		[FromQuery][StringLength(7)] string? delmode,
		[FromQuery][StringLength(4096)] string? filter,
		[FromQuery][StringLength(2048)] string? sort,
		[FromQuery][Range(1, 100)] int? pageSize,
		[FromQuery][Range(1, int.MaxValue)] int? pageNumber
	)
	{
		var softDelMode = Extensions.ParseSoftDelMode(delmode);
		var filterValues = FiltersHelper<User>.SerializeFromString<UserModel>(filter);
		var sortParsed = (IEnumerable<FieldSortOrderItem>?)null;

		var totalRecords = await _userService.GetCountAsync(softDelMode, filterValues);

		var items = await _userService.GetAsync(softDelMode, filterValues, sortParsed, pageSize ?? 10, pageNumber ?? 1);
		var mapped = _mapper.Map<IEnumerable<UserModel>>(items) ?? Enumerable.Empty<UserModel>();

		return Ok(new PagedResponse<IEnumerable<UserModel>>(mapped, pageSize ?? 10, pageNumber ?? 1, (int)totalRecords));
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<UserModel>> GetAsync(int id)
	{
		var item = await _userService.GetAsync(id);
		if (item is null) return NotFound();
		var mapped = _mapper.Map<UserModel>(item);
		return Ok(mapped);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<ActionResult<int>> CreateAsync([FromBody] UserCreateModel model)
	{
		var dto = _mapper.Map<UserCreateDto>(model);
		var id = await _userService.CreateAsync(dto);
		return CreatedAtAction(nameof(CreateAsync), new { id = id });
	}

	[HttpPut("{id}")]
	public async Task<ActionResult> UpdateAsync(int id, [FromBody] UserUpdateModel model)
	{
		var dto = _mapper.Map<UserUpdateDto>(model);
		await _userService.UpdateAsync(id, dto);
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteAsync(int id)
	{
		await _userService.DeleteAsync(id);
		return NoContent();
	}

	[HttpPatch("{id}")]
	public async Task<ActionResult> RestoreAsync(int id)
	{
		await _userService.RestoreAsync(id);
		return NoContent();
	}
}

/* 		var aint = new int[] { 1, 2, 3, 7, 8, 100, 200, 300 };
		var astr = new string?[] { "a", null, "ccc" };

		var filterItems = new FieldFilterValue[]
		{
			new FieldFilterValue("Id") { Operation = FilterOperations.Equal, Value = 1 },
			new FieldFilterValue("Updated") { Operation = FilterOperations.Equal, Value = DateTimeOffset.UtcNow },
			new FieldFilterValue("Updated") { Operation = FilterOperations.Equal, Value = (DateTimeOffset?)null },
			new FieldFilterValue("Updated") { Operation = FilterOperations.Equal | FilterOperations.TrueWhenNull, Value = DateTimeOffset.UtcNow },
			new FieldFilterValue("Username") { Operation = FilterOperations.Equal, Value = "Admin" },
			new FieldFilterValue("Username") { Operation = FilterOperations.Equal | FilterOperations.CaseInsensitive, Value = "aDmin" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.Equal, Value = "Admin" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.Equal, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.Equal | FilterOperations.CaseInsensitive, Value = "aDmin" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.Equal | FilterOperations.CaseInsensitive, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.Equal | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, Value = "adMin" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.Equal | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.Equal | FilterOperations.TrueWhenNull, Value = "admIn" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.Equal | FilterOperations.TrueWhenNull, Value = (string?) null },

 			new FieldFilterValue("Id") { Operation = FilterOperations.NotEqual, Value = 1 },
			new FieldFilterValue("Updated") { Operation = FilterOperations.NotEqual, Value = DateTimeOffset.UtcNow },
			new FieldFilterValue("Updated") { Operation = FilterOperations.NotEqual, Value = (DateTimeOffset?)null },
			new FieldFilterValue("Updated") { Operation = FilterOperations.NotEqual | FilterOperations.TrueWhenNull, Value = DateTimeOffset.UtcNow },
			new FieldFilterValue("Username") { Operation = FilterOperations.NotEqual, Value = "Admin" },
			new FieldFilterValue("Username") { Operation = FilterOperations.NotEqual | FilterOperations.CaseInsensitive, Value = "aDmin" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.NotEqual, Value = "Admin" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.NotEqual, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.NotEqual | FilterOperations.CaseInsensitive, Value = "aDmin" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.NotEqual | FilterOperations.CaseInsensitive, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.NotEqual | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, Value = "adMin" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.NotEqual | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.NotEqual | FilterOperations.TrueWhenNull, Value = "admIn" },
			new FieldFilterValue("FirstName") { Operation = FilterOperations.NotEqual | FilterOperations.TrueWhenNull, Value = (string?) null },

			new FieldFilterValue("Id") { Operation = FilterOperations.Greater, Value = 3 },
			new FieldFilterValue("Id") { Operation = FilterOperations.GreaterOrEqual, Value = 4 },
			new FieldFilterValue("Id") { Operation = FilterOperations.Less, Value = 5 },
			new FieldFilterValue("Id") { Operation = FilterOperations.LessOrEqual, Value = 6 },

			new FieldFilterValue("LastName") { Operation = FilterOperations.IsNull },
			new FieldFilterValue("LastName") { Operation = FilterOperations.IsNotNull },
			new FieldFilterValue("Deleted") { Operation = FilterOperations.IsNull },
			new FieldFilterValue("Deleted") { Operation = FilterOperations.IsNotNull },

			new FieldFilterValue("Id") { Operation = FilterOperations.Between, Value = 1, Value2 = int.MaxValue },
			new FieldFilterValue("Updated") { Operation = FilterOperations.Between | FilterOperations.TrueWhenNull,
				Value = new DateTimeOffset(2000, 8, 29, 4, 20, 57, TimeSpan.Zero),
				Value2 = new DateTimeOffset(2043, 01, 31, 12, 24, 19, TimeSpan.Zero)
			},
			new FieldFilterValue("Id") { Operation = FilterOperations.NotBetween, Value = 7, Value2 = 9 },
			new FieldFilterValue("Deleted") { Operation = FilterOperations.NotBetween | FilterOperations.TrueWhenNull,
				Value = new DateTimeOffset(2000, 8, 29, 4, 20, 57, TimeSpan.Zero),
				Value2 = new DateTimeOffset(2043, 01, 31, 12, 24, 19, TimeSpan.Zero)
			},
			
			new FieldFilterValue("Id") { Operation = FilterOperations.In, Value = aint },
			new FieldFilterValue("Id") { Operation = FilterOperations.NotIn, Value = aint },

			new FieldFilterValue("FirstName") { Operation = FilterOperations.In, Value = astr },
			new FieldFilterValue("LastName") { Operation = FilterOperations.NotIn, Value = astr },

			new FieldFilterValue("Username") { Operation = FilterOperations.Like, Value = "DM" },
			new FieldFilterValue("Username") { Operation = FilterOperations.Like | FilterOperations.CaseInsensitive, Value = "DM" },
			new FieldFilterValue("Username") { Operation = FilterOperations.NotLike, Value = "DM" },
			new FieldFilterValue("Username") { Operation = FilterOperations.NotLike | FilterOperations.CaseInsensitiveInvariant, Value = "DM" },

			new FieldFilterValue("Id") { Operation = FilterOperations.BitsAnd, Value = 3 },
			new FieldFilterValue("Id") { Operation = FilterOperations.BitsOr, Value = 6 },
		};

		var condition = FiltersHelper<User, UserDto>.BuildCondition(filterItems);
		query = query.Where(condition);

		Console.WriteLine(query.GetExecutionModel().ToString()); */