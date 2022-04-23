using AutoMapper;
using GoodsKB.BLL.Common;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Exceptions;
using GoodsKB.DAL;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories;
using GoodsKB.DAL.Repositories.Filter;
using GoodsKB.DAL.Repositories.Mongo;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GoodsKB.BLL.Services;

public interface IUserService
{
	Task<long> GetCountAsync(SoftDelModes mode, IFieldFilter<User>? filter);
	Task<IEnumerable<UserDto>> GetAsync(SoftDelModes mode, IFieldFilter<User>? filter, IEntitySort<User>? sort, int pageSize, int pageNumber);
	Task<UserDto> GetAsync(int id);
	Task<int> CreateAsync(UserCreateDto dto);
	Task UpdateAsync(int id, UserUpdateDto dto);
	Task DeleteAsync(int id);
	Task RestoreAsync(int id);
}

public class UserService : IUserService
{
	private readonly IMapper _mapper;
	private readonly IDALContext _context;
	private readonly IMongoSoftDelRepo<int, User, DateTimeOffset> _items;

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

		return (await _items.CreateAsync(item)).Id;
	}

	public async Task<long> GetCountAsync(SoftDelModes mode, IFieldFilter<User>? filter)
	{
		return await _items.GetCountAsync(mode);
	}

	public async Task<IEnumerable<UserDto>> GetAsync(SoftDelModes mode, IFieldFilter<User>? filter, IEntitySort<User>? sort, int pageSize, int pageNumber)
	{
		var query = _items.GetMongoEntities(SoftDelModes.All);//(mode);
		//if (filter != null) filter.Apply(query);
		if (sort != null) sort.Apply(query);

		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal, x => x.Id, 1).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal, x => x.Updated, DateTimeOffset.UtcNow).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal, x => x.Updated, (DateTimeOffset?)null).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.TrueWhenNull, x => x.Updated, DateTimeOffset.UtcNow).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal, x => x.Username, "Admin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.CaseInsensitive, x => x.Username, "aDmin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, x => x.Username, "adMin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.TrueWhenNull, x => x.Username, "admIn").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal, x => x.FirstName, "Admin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal, x => x.FirstName, (string?) null).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.CaseInsensitive, x => x.FirstName, "aDmin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.CaseInsensitive, x => x.FirstName, (string?) null).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, x => x.FirstName, "adMin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, x => x.FirstName, (string?) null).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.TrueWhenNull, x => x.FirstName, "admIn").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Equal | FilterOperations.TrueWhenNull, x => x.FirstName, (string?) null).Condition).GetExecutionModel().ToString());

		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual, x => x.Id, 1).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual, x => x.Updated, DateTimeOffset.UtcNow).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual, x => x.Updated, (DateTimeOffset?)null).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.TrueWhenNull, x => x.Updated, DateTimeOffset.UtcNow).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual, x => x.Username, "Admin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.CaseInsensitive, x => x.Username, "aDmin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, x => x.Username, "adMin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.TrueWhenNull, x => x.Username, "admIn").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual, x => x.FirstName, "Admin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual, x => x.FirstName, (string?) null).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.CaseInsensitive, x => x.FirstName, "aDmin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.CaseInsensitive, x => x.FirstName, (string?) null).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, x => x.FirstName, "adMin").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.CaseInsensitive | FilterOperations.TrueWhenNull, x => x.FirstName, (string?) null).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.TrueWhenNull, x => x.FirstName, "admIn").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotEqual | FilterOperations.TrueWhenNull, x => x.FirstName, (string?) null).Condition).GetExecutionModel().ToString());

		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Greater, x => x.Id, 3).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.GreaterOrEqual, x => x.Id, 4).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Less, x => x.Id, 5).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.LessOrEqual, x => x.Id, 6).Condition).GetExecutionModel().ToString());

		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.IsNull, x => x.LastName).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.IsNotNull, x => x.LastName).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.IsNull, x => x.Deleted).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.IsNotNull, x => x.Deleted).Condition).GetExecutionModel().ToString());

		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Between, x => x.Id, 1, int.MaxValue).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Between | FilterOperations.TrueWhenNull, x => x.Updated,
				new DateTimeOffset(2000, 8, 29, 4, 20, 57, TimeSpan.Zero),
				new DateTimeOffset(2043, 01, 31, 12, 24, 19, TimeSpan.Zero)
			).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotBetween, x => x.Id, 7, 9).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotBetween | FilterOperations.TrueWhenNull, x => x.Deleted,
				new DateTimeOffset(2000, 8, 29, 4, 20, 57, TimeSpan.Zero),
				new DateTimeOffset(2043, 01, 31, 12, 24, 19, TimeSpan.Zero)
			).Condition).GetExecutionModel().ToString());

		var aint = new int[] { 1, 2, 3, 7, 8, 100, 200, 300 };
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.In, x => x.Id, aint).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotIn, x => x.Id, aint).Condition).GetExecutionModel().ToString());

		var astr = new string?[] { "a", null, "ccc" };
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.In, x => x.FirstName, astr).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotIn, x => x.LastName, astr).Condition).GetExecutionModel().ToString());

		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Like, x => x.Username, "DM").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.Like | FilterOperations.CaseInsensitive, x => x.Username, "DM").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotLike, x => x.Username, "DM").Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.NotLike | FilterOperations.CaseInsensitiveInvariant, x => x.Username, "DM").Condition).GetExecutionModel().ToString());

		Console.WriteLine(query.Skip((pageNumber - 1) * pageSize).Take(pageSize).GetExecutionModel().ToString());

		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.BitsAnd, x => x.Id, 3).Condition).GetExecutionModel().ToString());
		Console.WriteLine(query.Where(FieldPredicate<User>.Build(FilterOperations.BitsOr, x => x.Id, 6).Condition).GetExecutionModel().ToString());

		Console.WriteLine(query.GetExecutionModel().ToString());

		var items = await query.ToListAsync();
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

	public async Task UpdateAsync(int id, UserUpdateDto dto)
	{
		var old = await _items.GetAsync(id);
		if (old == null)
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

		MongoDB.Driver.FilterDefinition<User> filter;
		if (email != null && phone != null)
			filter = _items.Filter.Eq(x => x.Username, username) | _items.Filter.Eq(x => x.Email, email) | _items.Filter.Eq(x => x.Phone, phone);
		else if (email != null)
			filter = _items.Filter.Eq(x => x.Username, username) | _items.Filter.Eq(x => x.Email, email);
		else if (phone != null)
			filter = _items.Filter.Eq(x => x.Username, username) | _items.Filter.Eq(x => x.Phone, phone);
		else
			throw new Conflict409Exception($"Email or phone must be provided");

		if ((await _items.GetAsync(SoftDelModes.All, x => x.Id != id && (x.Username == username || x.Email == email || x.Phone == phone))).SingleOrDefault() != null)
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
		if (!await _items.DeleteAsync(id))
			throw new NotFound404Exception();
	}

	public async Task RestoreAsync(int id)
	{
		if (!await _items.RestoreAsync(id))
			throw new NotFound404Exception();
	}
}