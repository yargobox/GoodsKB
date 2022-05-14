namespace GoodsKB.API.Controllers;

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GoodsKB.API.Filters;
using GoodsKB.API.Helpers;
using GoodsKB.API.Models;
using GoodsKB.API.SortOrders;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using SoftDel = GoodsKB.DAL.Repositories.SoftDel;

[ApiController]
[Route("api/[controller]s")]
[Produces("application/json")]
public class BrandController : ControllerBase
{
	private readonly IBrandService _service;
	private readonly IMapper _mapper;
	private readonly ILogger<BrandController> _logger;

	public BrandController(IBrandService service, IMapper mapper, ILogger<BrandController> logger)
	{
		_service = service;
		_mapper = mapper;
		_logger = logger;
	}

	[HttpGet]
	[ProducesResponseType(typeof(PagedResponse<IEnumerable<BrandModel>>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAsync(
		[FromQuery][StringLength(7)] string? del,
		[FromQuery][StringLength(4096)] string? filter,
		[FromQuery][StringLength(2048)] string? sort,
		[FromQuery][Range(1, 100)] int? psize,
		[FromQuery][Range(1, int.MaxValue)] int? pnum
	)
	{
		psize ??= 10;
		pnum ??= 1;
		var softDel = SoftDelHelper.TryParse(del, SoftDel.Actual);
		var filterValues = FiltersHelper.SerializeFromString(_service.GetFilters<BrandModel>(), filter);
		var sortOrderValues = SortOrdersHelper.SerializeFromString(_service.GetSortOrders<BrandModel>(), sort);
		
		var totalRecords = await _service.GetCountAsync(softDel, filterValues);
		
		IEnumerable<BrandModel> mapped;
		if (totalRecords > 0)
		{
			var items = await _service.GetAsync(softDel, filterValues, sortOrderValues, psize.Value, pnum.Value);
			mapped = _mapper.Map<IEnumerable<BrandModel>>(items);
		}
		else
		{
			mapped = Enumerable.Empty<BrandModel>();
		}

		return Ok(new PagedResponse<BrandModel>(mapped, psize.Value, pnum.Value, totalRecords));
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<BrandModel>> GetAsync(int id)
	{
		var item = await _service.GetAsync(id);
		if (item is null) return NotFound();
		var mapped = _mapper.Map<BrandModel>(item);
		return Ok(mapped);
	}

	[HttpPost]
	//[ValidateAntiForgeryToken]
	public async Task<ActionResult<int?>> CreateAsync([FromBody] BrandCreateModel model)
	{
		var dto = _mapper.Map<BrandCreateDto>(model);
		var id = await _service.CreateAsync(dto);
		return CreatedAtAction(nameof(CreateAsync), new { id = id });
	}

	[HttpPut("{id}")]
	public async Task<ActionResult> UpdateAsync(int id, [FromBody] BrandUpdateModel model)
	{
		var dto = _mapper.Map<BrandUpdateDto>(model);
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