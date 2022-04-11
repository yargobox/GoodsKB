using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Services;
using GoodsKB.API.Models;

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
	public async Task<IEnumerable<UserModel>> GetAsync()
	{
		var items = await _userService.GetAsync();
		var mapped = _mapper.Map<IEnumerable<UserModel>>(items);
		return await Task.FromResult(mapped);
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
		if (dto.Id == 0)
			dto.Id = id;
		else if (dto.Id != id)
			return Conflict();
		await _userService.UpdateAsync(dto);
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteAsync(int id)
	{
		await _userService.DeleteAsync(id);
		return NoContent();
	}
}