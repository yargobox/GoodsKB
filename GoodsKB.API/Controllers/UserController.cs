namespace GoodsKB.API.Controllers;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using AutoMapper;
using GoodsKB.API.Filters;
using GoodsKB.API.Helpers;
using GoodsKB.API.Models;
using GoodsKB.API.SortOrders;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Services;
using GoodsKB.DAL.Repositories.Mongo;
using Microsoft.AspNetCore.Mvc;
using SoftDel = GoodsKB.DAL.Repositories.SoftDel;
using User = GoodsKB.DAL.Entities.User;

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

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		///
		/* var q = _service.Q;
		var c = _service.C;
		var next = false;
		var orders = _service.GetSortOrders<UserModel>()["Id"];
		var orderValue = new GoodsKB.DAL.Repositories.SortOrders.SortOrderValue("Id") { Operation = DAL.Repositories.SortOrders.SO.Ascending };
		var entityParameter = Expression.Parameter(typeof(User), "item");
		var propInfo = typeof(User).GetProperty(orderValue.PropertyName)
			?? throw new InvalidOperationException($"{typeof(User).Name} does not have a property {orderValue.PropertyName}.");
		var propMember = Expression.MakeMemberAccess(entityParameter, propInfo);
		var orderByExp = Expression.Lambda(propMember, entityParameter);
		var methodName = next ?
			(orderValue.Operation.HasFlag(DAL.Repositories.SortOrders.SO.Ascending) ? "ThenBy" : "ThenByDescending") :
			(orderValue.Operation.HasFlag(DAL.Repositories.SortOrders.SO.Ascending) ? "OrderBy" : "OrderByDescending");
		var predicate = Expression.Call(
			typeof(Queryable),
			methodName,
			new Type[] { typeof(User), propInfo.PropertyType },
			q.Expression,
			Expression.Quote(orderByExp));

		q = q.Provider.CreateQuery<User>(predicate);

		var o1 = MongoDB.Driver.Builders<User>.Sort.Ascending(x => x.Id);
		var o2 = MongoDB.Driver.Builders<User>.Sort.Ascending(x => x.Deleted);
		var o3 = MongoDB.Driver.Builders<User>.Sort.Combine(o1, o2);

		var s = o3.Render(c.DocumentSerializer, c.Settings.SerializerRegistry).ToString();
		Console.WriteLine(s);
		Console.WriteLine(q.ToString());

		MongoDB.Driver.SortDefinition<User> o4 = @"{ ""_id"" : 1, ""Deleted"" : 1 }";
		var s = o4.Render(c.DocumentSerializer, c.Settings.SerializerRegistry).ToString();
		Console.WriteLine(s);

		Test<User>(OrderBy.Asc(x => x.Deleted).Desc(x => x.Created)); */

		///
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		pageSize ??= 10;
		pageNumber ??= 1;
		var softDelMode = SoftDelHelper.TryParse(delmode, SoftDel.Actual);
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