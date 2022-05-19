namespace GoodsKB.API.Controllers;

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GoodsKB.API.Filters;
using GoodsKB.API.Helpers;
using GoodsKB.API.Models;
using GoodsKB.API.SortOrders;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Services;
using GoodsKB.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using SoftDel = GoodsKB.DAL.Repositories.SoftDel;

[ApiController]
[Route("api/[controller]s")]
[Produces("application/json")]
public class Example2Controller : ControllerBase
{
	private readonly IExamples2Service _service;
	private readonly IMapper _mapper;
	private readonly ILogger<Example2Controller> _logger;

	public Example2Controller(IExamples2Service service, IMapper mapper, ILogger<Example2Controller> logger)
	{
		_service = service;
		_mapper = mapper;
		_logger = logger;
	}

	[HttpGet]
	[ProducesResponseType(typeof(PagedResponse<IEnumerable<Example2Model>>), StatusCodes.Status200OK)]
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
		var filterValues = FiltersHelper.SerializeFromString(_service.GetFilters<Example2Model>(), filter);
		var sortOrderValues = SortOrdersHelper.SerializeFromString(_service.GetSortOrders<Example2Model>(), sort);
		
		var totalRecords = await _service.GetCountAsync(softDel, filterValues);
		
		IEnumerable<Example2Model> mapped;
		if (totalRecords > 0)
		{
			var items = await _service.GetAsync(softDel, filterValues, sortOrderValues, psize.Value, pnum.Value);
			mapped = _mapper.Map<IEnumerable<Example2Model>>(items);
		}
		else
		{
			mapped = Enumerable.Empty<Example2Model>();
		}

		return Ok(new PagedResponse<Example2Model>(mapped, psize.Value, pnum.Value, totalRecords));
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Example2Model>> GetAsync(string id)
	{
		var example2Id = Example2Id.Create.FromStrng(id);
		var item = await _service.GetAsync(example2Id);
		if (item is null) return NotFound();
		var mapped = _mapper.Map<Example2Model>(item);
		return Ok(mapped);
	}

	[HttpPost]
	//[ValidateAntiForgeryToken]
	public async Task<ActionResult<Example2Id>> CreateAsync([FromBody] Example2CreateModel model)
	{
		var dto = _mapper.Map<Example2CreateDto>(model);
		var id = await _service.CreateAsync(dto);
		return CreatedAtAction(nameof(CreateAsync), new { id = id });
	}

	[HttpPut("{id}")]
	public async Task<ActionResult> UpdateAsync(string id, [FromBody] Example2UpdateModel model)
	{
		var example2Id = Example2Id.Create.FromStrng(id);
		var dto = _mapper.Map<Example2UpdateDto>(model);
		await _service.UpdateAsync(example2Id, dto);
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteAsync(string id)
	{
		var example2Id = Example2Id.Create.FromStrng(id);
		await _service.DeleteAsync(example2Id);
		return NoContent();
	}

	[HttpPatch("{id}")]
	public async Task<ActionResult> RestoreAsync(string id)
	{
		var example2Id = Example2Id.Create.FromStrng(id);
		await _service.RestoreAsync(example2Id);
		return NoContent();
	}
}