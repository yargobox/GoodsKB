namespace GoodsKB.BLL.Services;

using AutoMapper;
using GoodsKB.BLL.DTOs;
using GoodsKB.BLL.Exceptions;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Linq;
using GoodsKB.DAL.Repositories;
using GoodsKB.DAL.Repositories.Filters;
using GoodsKB.DAL.Repositories.Mongo;
using GoodsKB.DAL.Repositories.SortOrders;

public interface IBrandService
{
	IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull;
	IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>() where TDto : notnull;
	Task<long> GetCountAsync(SoftDel mode, FilterValues? filter);
	Task<IEnumerable<BrandDto>> GetAsync(SoftDel mode, FilterValues? filter, SortOrderValues? sort, int pageSize, int pageNumber);
	Task<BrandDto> GetAsync(int id);
	Task<int> CreateAsync(BrandCreateDto dto);
	Task UpdateAsync(int id, BrandUpdateDto dto);
	Task DeleteAsync(int id);
	Task RestoreAsync(int id);
}

public class BrandService : IBrandService
{
	private readonly IMapper _mapper;
	private readonly IDALContext _context;
	private readonly ISoftDelRepoMongo<int?, Brand, DateTimeOffset> _repo;

	public BrandService(IDALContext context, IMapper mapper)
	{
		_mapper = mapper;
		_context = context;
		_repo = _context.Brands;
	}

	public IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull
		=> FilterConditionBuilder.GetFilters<Brand, TDto>();
	public IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>()  where TDto : notnull
		=> SortOrderConditionBuilder.GetSortOrders<Brand, TDto>();

	public async Task<long> GetCountAsync(SoftDel mode, FilterValues? filter)
	{
		var cond = filter.BuildCondition<Brand>();
		return await _repo.GetCountAsync(mode, cond);
	}

	public async Task<IEnumerable<BrandDto>> GetAsync(SoftDel mode, FilterValues? filter, SortOrderValues? sort, int pageSize, int pageNumber)
	{
		var items = await _repo.GetQuery(mode)
			.Where(filter)
			.OrderBy(sort)
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

		return _mapper.Map<IEnumerable<BrandDto>>(items);
	}

	public async Task<BrandDto> GetAsync(int id)
	{
		var item = await _repo.GetAsync(id) ??
			throw new NotFound404Exception($"A brand [{id}] does not exist.");

		return _mapper.Map<BrandDto>(item);
	}

	public async Task<int> CreateAsync(BrandCreateDto dto)
	{
		var name = !string.IsNullOrWhiteSpace(dto.Name) ? dto.Name.Trim() :
			throw new Conflict409Exception($"A brand name must be provided.");

		var desc = !string.IsNullOrWhiteSpace(dto.Desc) ? dto.Desc.Trim() : null;

		if (await _repo.GetCountAsync(SoftDel.All, x => x.Name!.ToLower() == name.ToLower()) > 0)
			throw new Conflict409Exception($"A brand {name} already exists.");

		var item = _mapper.Map<Brand>(dto);
		item.Name = name;
		item.Desc = desc;

		return (await _repo.CreateAsync(item)).Id!.Value;
	}
	public async Task UpdateAsync(int id, BrandUpdateDto dto)
	{
		var item = await _repo.GetAsync(id) ??
			throw new NotFound404Exception($"A brand {dto.Name} [{id}] does not exist or has been deleted.");

		var name = !string.IsNullOrWhiteSpace(dto.Name) ? dto.Name.Trim() :
			throw new Conflict409Exception($"A brand name must be provided.");

		var desc = !string.IsNullOrWhiteSpace(dto.Desc) ? dto.Desc.Trim() : null;

		if (item.Name != name && await _repo.GetCountAsync(SoftDel.All, x => x.Name!.ToLower() == name.ToLower()) > 0)
			throw new Conflict409Exception($"A brand {name} already exists.");

		item.Name = name;
		item.Desc = desc;

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