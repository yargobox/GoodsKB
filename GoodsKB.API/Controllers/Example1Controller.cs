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
public class Example1Controller : ControllerBase
{
	private readonly IExamples1Service _service;
	private readonly IMapper _mapper;
	private readonly ILogger<Example1Controller> _logger;

	public Example1Controller(IExamples1Service service, IMapper mapper, ILogger<Example1Controller> logger)
	{
		_service = service;
		_mapper = mapper;
		_logger = logger;
	}

	[HttpGet]
	[ProducesResponseType(typeof(PagedResponse<IEnumerable<Example1Model>>), StatusCodes.Status200OK)]
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
		var filterValues = FiltersHelper.SerializeFromString(_service.GetFilters<Example1Model>(), filter);
		var sortOrderValues = SortOrdersHelper.SerializeFromString(_service.GetSortOrders<Example1Model>(), sort);
		
		var totalRecords = await _service.GetCountAsync(softDel, filterValues);
		
		IEnumerable<Example1Model> mapped;
		if (totalRecords > 0)
		{
			var items = await _service.GetAsync(softDel, filterValues, sortOrderValues, psize.Value, pnum.Value);
			mapped = _mapper.Map<IEnumerable<Example1Model>>(items);
		}
		else
		{
			mapped = Enumerable.Empty<Example1Model>();
		}

		return Ok(new PagedResponse<Example1Model>(mapped, psize.Value, pnum.Value, totalRecords));
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Example1Model>> GetAsync(string id)
	{
		var example1Id = Example1Id.Create.FromStrng(id);
		var item = await _service.GetAsync(example1Id);
		if (item is null) return NotFound();
		var mapped = _mapper.Map<Example1Model>(item);
		return Ok(mapped);
	}

	[HttpPost]
	//[ValidateAntiForgeryToken]
	public async Task<ActionResult<Example1Id>> CreateAsync([FromBody] Example1CreateModel model)
	{
		var dto = _mapper.Map<Example1CreateDto>(model);
		var id = await _service.CreateAsync(dto);
		return CreatedAtAction(nameof(CreateAsync), new { id = id });
	}

	[HttpPut("{id}")]
	public async Task<ActionResult> UpdateAsync(string id, [FromBody] Example1UpdateModel model)
	{
		var example1Id = Example1Id.Create.FromStrng(id);
		var dto = _mapper.Map<Example1UpdateDto>(model);
		await _service.UpdateAsync(example1Id, dto);
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteAsync(string id)
	{
		var example1Id = Example1Id.Create.FromStrng(id);
		await _service.DeleteAsync(example1Id);
		return NoContent();
	}

	[HttpPatch("{id}")]
	public async Task<ActionResult> RestoreAsync(string id)
	{
		var example1Id = Example1Id.Create.FromStrng(id);
		await _service.RestoreAsync(example1Id);
		return NoContent();
	}
}