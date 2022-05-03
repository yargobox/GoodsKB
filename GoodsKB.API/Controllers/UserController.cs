using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GoodsKB.API.Filters;
using GoodsKB.API.Models;
using GoodsKB.BLL.Common;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Services;
using GoodsKB.DAL.Repositories.Filters;
using Microsoft.AspNetCore.Mvc;
using SoftDelModes = GoodsKB.DAL.Repositories.SoftDelModes;
using User = GoodsKB.DAL.Entities.User;

namespace GoodsKB.API.Controllers;

[ApiController]
[Route("api/[controller]s")]
[Produces("application/json")]
public class UserController : ControllerBase
{
	private readonly IUserService _service;
	private readonly IMapper _mapper;
	private readonly ILogger<UserController> _logger;

	public UserController(IUserService service, IMapper mapper, ILogger<UserController> logger)
	{
		_service = service;
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
		var filterValues = FiltersHelper.SerializeFromString(
			_service.GetFilters<UserModel>(),
			filter ?? string.Empty);
		var sortParsed = (IEnumerable<FieldSortOrderItem>?)null;

		var totalRecords = await _service.GetCountAsync(softDelMode, filterValues);

		var items = await _service.GetAsync(softDelMode, filterValues, sortParsed, pageSize ?? 10, pageNumber ?? 1);
		var mapped = _mapper.Map<IEnumerable<UserModel>>(items) ?? Enumerable.Empty<UserModel>();

		return Ok(new PagedResponse<IEnumerable<UserModel>>(mapped, pageSize ?? 10, pageNumber ?? 1, (int)totalRecords));
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<UserModel>> GetAsync(int id)
	{
		var item = await _service.GetAsync(id);
		if (item is null) return NotFound();
		var mapped = _mapper.Map<UserModel>(item);
		return Ok(mapped);
	}

	[HttpPost]
	//[ValidateAntiForgeryToken]
	public async Task<ActionResult<int>> CreateAsync([FromBody] UserCreateModel model)
	{
		var dto = _mapper.Map<UserCreateDto>(model);
		var id = await _service.CreateAsync(dto);
		return CreatedAtAction(nameof(CreateAsync), new { id = id });
	}

	[HttpPut("{id}")]
	public async Task<ActionResult> UpdateAsync(int id, [FromBody] UserUpdateModel model)
	{
		var dto = _mapper.Map<UserUpdateDto>(model);
		await _service.UpdateAsync(id, dto);
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteAsync(int id)
	{
		await _service.DeleteAsync(id);
		return NoContent();
	}

	[HttpPatch("{id}")]
	public async Task<ActionResult> RestoreAsync(int id)
	{
		await _service.RestoreAsync(id);
		return NoContent();
	}
}

/* 		var aint = new int[] { 1, 2, 3, 7, 8, 100, 200, 300 };
		var astr = new string?[] { "a", null, "ccc" };

		var filterItems = new FieldFilterValue[]
		{
			new FieldFilterValue("Id") { Operation = FO.Equal, Value = 1 },
			new FieldFilterValue("Updated") { Operation = FO.Equal, Value = DateTimeOffset.UtcNow },
			new FieldFilterValue("Updated") { Operation = FO.Equal, Value = (DateTimeOffset?)null },
			new FieldFilterValue("Updated") { Operation = FO.Equal | FO.TrueWhenNull, Value = DateTimeOffset.UtcNow },
			new FieldFilterValue("Username") { Operation = FO.Equal, Value = "Admin" },
			new FieldFilterValue("Username") { Operation = FO.Equal | FO.CaseInsensitive, Value = "aDmin" },
			new FieldFilterValue("FirstName") { Operation = FO.Equal, Value = "Admin" },
			new FieldFilterValue("FirstName") { Operation = FO.Equal, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FO.Equal | FO.CaseInsensitive, Value = "aDmin" },
			new FieldFilterValue("FirstName") { Operation = FO.Equal | FO.CaseInsensitive, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FO.Equal | FO.CaseInsensitive | FO.TrueWhenNull, Value = "adMin" },
			new FieldFilterValue("FirstName") { Operation = FO.Equal | FO.CaseInsensitive | FO.TrueWhenNull, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FO.Equal | FO.TrueWhenNull, Value = "admIn" },
			new FieldFilterValue("FirstName") { Operation = FO.Equal | FO.TrueWhenNull, Value = (string?) null },

 			new FieldFilterValue("Id") { Operation = FO.NotEqual, Value = 1 },
			new FieldFilterValue("Updated") { Operation = FO.NotEqual, Value = DateTimeOffset.UtcNow },
			new FieldFilterValue("Updated") { Operation = FO.NotEqual, Value = (DateTimeOffset?)null },
			new FieldFilterValue("Updated") { Operation = FO.NotEqual | FO.TrueWhenNull, Value = DateTimeOffset.UtcNow },
			new FieldFilterValue("Username") { Operation = FO.NotEqual, Value = "Admin" },
			new FieldFilterValue("Username") { Operation = FO.NotEqual | FO.CaseInsensitive, Value = "aDmin" },
			new FieldFilterValue("FirstName") { Operation = FO.NotEqual, Value = "Admin" },
			new FieldFilterValue("FirstName") { Operation = FO.NotEqual, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FO.NotEqual | FO.CaseInsensitive, Value = "aDmin" },
			new FieldFilterValue("FirstName") { Operation = FO.NotEqual | FO.CaseInsensitive, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FO.NotEqual | FO.CaseInsensitive | FO.TrueWhenNull, Value = "adMin" },
			new FieldFilterValue("FirstName") { Operation = FO.NotEqual | FO.CaseInsensitive | FO.TrueWhenNull, Value = (string?) null },
			new FieldFilterValue("FirstName") { Operation = FO.NotEqual | FO.TrueWhenNull, Value = "admIn" },
			new FieldFilterValue("FirstName") { Operation = FO.NotEqual | FO.TrueWhenNull, Value = (string?) null },

			new FieldFilterValue("Id") { Operation = FO.Greater, Value = 3 },
			new FieldFilterValue("Id") { Operation = FO.GreaterOrEqual, Value = 4 },
			new FieldFilterValue("Id") { Operation = FO.Less, Value = 5 },
			new FieldFilterValue("Id") { Operation = FO.LessOrEqual, Value = 6 },

			new FieldFilterValue("LastName") { Operation = FO.IsNull },
			new FieldFilterValue("LastName") { Operation = FO.IsNotNull },
			new FieldFilterValue("Deleted") { Operation = FO.IsNull },
			new FieldFilterValue("Deleted") { Operation = FO.IsNotNull },

			new FieldFilterValue("Id") { Operation = FO.Between, Value = 1, Value2 = int.MaxValue },
			new FieldFilterValue("Updated") { Operation = FO.Between | FO.TrueWhenNull,
				Value = new DateTimeOffset(2000, 8, 29, 4, 20, 57, TimeSpan.Zero),
				Value2 = new DateTimeOffset(2043, 01, 31, 12, 24, 19, TimeSpan.Zero)
			},
			new FieldFilterValue("Id") { Operation = FO.NotBetween, Value = 7, Value2 = 9 },
			new FieldFilterValue("Deleted") { Operation = FO.NotBetween | FO.TrueWhenNull,
				Value = new DateTimeOffset(2000, 8, 29, 4, 20, 57, TimeSpan.Zero),
				Value2 = new DateTimeOffset(2043, 01, 31, 12, 24, 19, TimeSpan.Zero)
			},
			
			new FieldFilterValue("Id") { Operation = FO.In, Value = aint },
			new FieldFilterValue("Id") { Operation = FO.NotIn, Value = aint },

			new FieldFilterValue("FirstName") { Operation = FO.In, Value = astr },
			new FieldFilterValue("LastName") { Operation = FO.NotIn, Value = astr },

			new FieldFilterValue("Username") { Operation = FO.Like, Value = "DM" },
			new FieldFilterValue("Username") { Operation = FO.Like | FO.CaseInsensitive, Value = "DM" },
			new FieldFilterValue("Username") { Operation = FO.NotLike, Value = "DM" },
			new FieldFilterValue("Username") { Operation = FO.NotLike | FO.CaseInsensitiveInvariant, Value = "DM" },

			new FieldFilterValue("Id") { Operation = FO.BitsAnd, Value = 3 },
			new FieldFilterValue("Id") { Operation = FO.BitsOr, Value = 6 },
		};

		var condition = FiltersHelper<User, UserDto>.BuildCondition(filterItems);
		query = query.Where(condition);

		Console.WriteLine(query.GetExecutionModel().ToString()); */