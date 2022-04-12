using GoodsKB.DAL;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Exceptions;
using GoodsKB.BLL.Common;
using AutoMapper;

namespace GoodsKB.BLL.Services;

public interface IUserService
{
	Task<IEnumerable<UserDto>> GetAsync();
	Task<UserDto> GetAsync(int id);
	Task<int> CreateAsync(UserCreateDto dto);
	Task UpdateAsync(UserUpdateDto dto);
	Task DeleteAsync(int id);
}

public class UserService : IUserService
{
	private readonly IMapper _mapper;
	private readonly IDALContext _context;
	private readonly IBaseRepo<int, User> _items;

	public UserService(IDALContext context, IMapper mapper)
	{
		_mapper = mapper;
		_context = context;
		_items = _context.Users;
	}

	public async Task<int> CreateAsync(UserCreateDto dto)
	{
		string? username = null;
		if (!string.IsNullOrWhiteSpace(dto.Username))
			username = dto.Username.Trim();
		else
			throw new Conflict409Exception($"Username must be provided");

		string? email = null;
		if (!string.IsNullOrWhiteSpace(dto.Email))
			email = dto.Email.Trim();

		string? phone = null;
		if (!string.IsNullOrWhiteSpace(dto.Phone))
			phone = dto.Phone.ToPhoneNumber();

		MongoDB.Driver.FilterDefinition<User> filter;
		if (email != null && phone != null)
			filter = _items.Filter.Eq(x => x.Username, username) | _items.Filter.Eq(x => x.Email, email) | _items.Filter.Eq(x => x.Phone, phone);
		else if (email != null)
			filter = _items.Filter.Eq(x => x.Username, username) | _items.Filter.Eq(x => x.Email, email);
		else if (phone != null)
			filter = _items.Filter.Eq(x => x.Username, username) | _items.Filter.Eq(x => x.Phone, phone);
		else
			throw new Conflict409Exception($"Email or phone must be provided");

        if ((await _items.GetAsync(filter)).FirstOrDefault() != null)
			throw new Conflict409Exception("The username, email or phone already exists");

		var item = _mapper.Map<User>(dto);
		item.Username = username;
		item.Email = email;
		item.Phone = phone;
		item.Created = DateTimeOffset.UtcNow;
		
		//item.Password = _authService.HashPassword(newUser.Password);

		return await _items.CreateAsync(item);
	}

	public async Task<IEnumerable<UserDto>> GetAsync()
	{
		var items = await _items.GetAsync();
		var mapped = _mapper.Map<IEnumerable<UserDto>>(items);
		return mapped;
	}

	public async Task<UserDto> GetAsync(int id)
	{
		var item = await _items.GetAsync(id);
		if (item == null) throw new NotFound404Exception($"User {id} do not exist");
		var mapped = _mapper.Map<UserDto>(item);
		return mapped;
	}

	public async Task UpdateAsync(UserUpdateDto dto)
	{
		string? username = null;
		if (!string.IsNullOrWhiteSpace(dto.Username))
			username = dto.Username.Trim();
		else
			throw new Conflict409Exception($"Username must be provided");

		string? email = null;
		if (!string.IsNullOrWhiteSpace(dto.Email))
			email = dto.Email.Trim();

		string? phone = null;
		if (!string.IsNullOrWhiteSpace(dto.Phone))
			phone = dto.Phone.ToPhoneNumber();

		MongoDB.Driver.FilterDefinition<User> filter;
		if (email != null && phone != null)
			filter = _items.Filter.Eq(x => x.Username, username) | _items.Filter.Eq(x => x.Email, email) | _items.Filter.Eq(x => x.Phone, phone);
		else if (email != null)
			filter = _items.Filter.Eq(x => x.Username, username) | _items.Filter.Eq(x => x.Email, email);
		else if (phone != null)
			filter = _items.Filter.Eq(x => x.Username, username) | _items.Filter.Eq(x => x.Phone, phone);
		else
			throw new Conflict409Exception($"Email or phone must be provided");

        if ((await _items.GetAsync(filter)).Where(x => x.Id != dto.Id).SingleOrDefault() != null)
			throw new Conflict409Exception("The username, email or phone already exists");

		var item = _mapper.Map<User>(dto);
		item.Username = username;
		item.Email = email;
		item.Phone = phone;
		item.Updated = DateTimeOffset.UtcNow;
		
		//item.Password = _authService.HashPassword(newUser.Password);

		await _items.UpdateAsync(item);
	}

	public async Task DeleteAsync(int id)
	{
		await _items.DeleteAsync(id);
	}
}