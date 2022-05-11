using System.Linq.Expressions;
using AutoMapper;
using GoodsKB.BLL.Common;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Exceptions;
using GoodsKB.DAL;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Linq;
using GoodsKB.DAL.Repositories;
using GoodsKB.DAL.Repositories.Filters;
using GoodsKB.DAL.Repositories.Mongo;
using GoodsKB.DAL.Repositories.SortOrders;

namespace GoodsKB.BLL.Services;

public interface IUserService
{
	IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull;
	IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>() where TDto : notnull;
	Task<long> GetCountAsync(SoftDel mode, FilterValues? filter);
	Task<IEnumerable<UserDto>> GetAsync(SoftDel mode, FilterValues? filter, SortOrderValues? sort, int pageSize, int pageNumber);
	Task<UserDto> GetAsync(int id);
	Task<int> CreateAsync(UserCreateDto dto);
	Task UpdateAsync(int id, UserUpdateDto dto);
	Task DeleteAsync(int id);
	Task RestoreAsync(int id);

	IQueryable<User> Q { get; }//!!!
	MongoDB.Driver.IMongoCollection<User> C { get; }//!!!
}

public class UserService : IUserService
{
	public IQueryable<User> Q => _repo.Query;
	public MongoDB.Driver.IMongoCollection<User> C => _repo.Collection;

	private readonly IMapper _mapper;
	private readonly IDALContext _context;
	private readonly ISoftDelRepoMongo<int, User, DateTimeOffset> _repo;

	public UserService(IDALContext context, IMapper mapper)
	{
		_mapper = mapper;
		_context = context;
		_repo = _context.Users;
	}

	public IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull => _repo.FilterBuilder.GetFilters<TDto>();
	public IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>() where TDto : notnull => _repo.SortOrderBuilder.GetSortOrders<TDto>();

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
			filter = _repo.Filter.Eq(x => x.Username, username) | _repo.Filter.Eq(x => x.Email, email) | _repo.Filter.Eq(x => x.Phone, phone);
		else if (email != null)
			filter = _repo.Filter.Eq(x => x.Username, username) | _repo.Filter.Eq(x => x.Email, email);
		else if (phone != null)
			filter = _repo.Filter.Eq(x => x.Username, username) | _repo.Filter.Eq(x => x.Phone, phone);
		else
			throw new Conflict409Exception($"Email or phone must be provided");

		if ((await _repo.GetAsync(filter)).FirstOrDefault() != null)
			throw new Conflict409Exception("The username, email or phone already exists");

		var item = _mapper.Map<User>(dto);
		item.Username = username;
		item.Email = email;
		item.Phone = phone;
		item.Created = DateTimeOffset.UtcNow;

		//item.Password = _authService.HashPassword(newUser.Password);

		return (await _repo.CreateAsync(item)).Id;
	}

	public async Task<long> GetCountAsync(SoftDel mode, FilterValues? filter)
	{
		var cond = (Expression<Func<User, bool>>?) _repo.FilterBuilder.BuildCondition(filter);
		return await _repo.GetCountAsync(mode, cond);
	}

	public async Task<IEnumerable<UserDto>> GetAsync(SoftDel mode, FilterValues? filter, SortOrderValues? sort, int pageSize, int pageNumber)
	{
		var query = _repo.GetQuery(mode);
		query = _repo.FilterBuilder.Apply(query, filter);
		query = _repo.SortOrderBuilder.Apply(query, sort);
		query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

		Console.WriteLine(((MongoDB.Driver.Linq.IMongoQueryable<User>)query).GetExecutionModel().ToString());//!!!

		//_repo.GetAsync()
		var items = await query.ToListAsync();
		var mapped = _mapper.Map<IEnumerable<UserDto>>(items);
		return mapped;
	}

	public async Task<UserDto> GetAsync(int id)
	{
		var item = await _repo.GetAsync(id);
		if (item == null) throw new NotFound404Exception($"User {id} do not exist");
		var mapped = _mapper.Map<UserDto>(item);
		return mapped;
	}

	public async Task UpdateAsync(int id, UserUpdateDto dto)
	{
		var item = await _repo.GetAsync(id);
		if (item == null)
			throw new NotFound404Exception($"User {dto.Username} [{id}] does not exist or has been deleted.");

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

		Expression<Func<User, bool>> filter;
		if (email != null && phone != null)
			filter = x => x.Id != id && (x.Username == username || x.Email == email || x.Phone == phone);
		else if (email != null)
			filter = x => x.Id != id && (x.Username == username || x.Email == email);
		else if (phone != null)
			filter = x => x.Id != id && (x.Username == username || x.Phone == phone);
		else
			throw new Conflict409Exception($"Email or phone must be provided");
			
		if ((await _repo.GetAsync(SoftDel.All, filter)).FirstOrDefault() != null)
			throw new Conflict409Exception("The username, email or phone already exist");

		//var item = _mapper.Map<User>(dto);
		item.Id = id;
		item.Username = username;
		item.Email = email;
		item.Phone = phone;
		item.Updated = DateTimeOffset.UtcNow;

		//item.Password = _authService.HashPassword(newUser.Password);

		if (!await _repo.UpdateAsync(item))
			throw new NotFound404Exception();
	}

	public async Task DeleteAsync(int id)
	{
		if (!await _repo.DeleteAsync(id))
			throw new NotFound404Exception();
	}

	public async Task RestoreAsync(int id)
	{
		if (!await _repo.RestoreAsync(id))
			throw new NotFound404Exception();
	}
}