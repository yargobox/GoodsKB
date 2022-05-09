using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GoodsKB.API.Filters;
using GoodsKB.API.Helpers;
using GoodsKB.API.Models;
using GoodsKB.API.SortOrders;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using SoftDelModes = GoodsKB.DAL.Repositories.SoftDelModes;

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
		pageSize ??= 10;
		pageNumber ??= 1;
		var softDelMode = SoftDelModeHelper.TryParse(delmode, SoftDelModes.Actual);
		var filterValues = FiltersHelper.SerializeFromString(_service.GetFilters<UserModel>(), filter);
		var sortOrderValues = SortOrdersHelper.SerializeFromString(_service.GetSortOrders<UserModel>(), sort);
		
		var totalRecords = await _service.GetCountAsync(softDelMode, filterValues);
		
		IEnumerable<UserModel> mapped;
		if (totalRecords > 0)
		{
			var items = await _service.GetAsync(softDelMode, filterValues, sortOrderValues, pageSize.Value, pageNumber.Value);
			mapped = _mapper.Map<IEnumerable<UserModel>>(items);
		}
		else
		{
			mapped = Enumerable.Empty<UserModel>();
		}

		return Ok(new PagedResponse<UserModel>(mapped, pageSize.Value, pageNumber.Value, totalRecords));
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