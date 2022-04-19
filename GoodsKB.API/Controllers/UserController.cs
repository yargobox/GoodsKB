using System.ComponentModel.DataAnnotations;
using AutoMapper;
using GoodsKB.API.Models;
using GoodsKB.BLL.Common;
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
		[FromQuery] [StringLength(7)] string? delmode,
		[FromQuery] [StringLength(4096)] string? filter,
		[FromQuery] [StringLength(2048)] string? sort,
		[FromQuery] [Range(1, 100)] int? pageSize,
		[FromQuery] [Range(1, int.MaxValue)] int? pageNumber
	)
	{
		var softDelMode = Extensions.ParseSoftDelMode(delmode);
		var totalRecords = await _userService.GetCountAsync(softDelMode, null);

		var items = await _userService.GetAsync(softDelMode, null, null, pageSize ?? 10, pageNumber ?? 1);
		var mapped = _mapper.Map<IEnumerable<UserModel>>(items) ?? Enumerable.Empty<UserModel>();

		return Ok(new PagedResponse<IEnumerable<UserModel>>(mapped, pageSize ?? 10, pageNumber ?? 1, (int) totalRecords));
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