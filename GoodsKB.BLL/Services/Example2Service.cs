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

public interface IExamples2Service
{
	IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull;
	IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>() where TDto : notnull;
	Task<long> GetCountAsync(SoftDel mode, FilterValues? filter);
	Task<IEnumerable<Example2Dto>> GetAsync(SoftDel mode, FilterValues? filter, SortOrderValues? sort, int pageSize, int pageNumber);
	Task<Example2Dto> GetAsync(Example2Id id);
	Task<Example2Id> CreateAsync(Example2CreateDto dto);
	Task UpdateAsync(Example2Id id, Example2UpdateDto dto);
	Task DeleteAsync(Example2Id id);
	Task RestoreAsync(Example2Id id);
}

public class Examples2Service : IExamples2Service
{
	private readonly IMapper _mapper;
	private readonly IDALContext _context;
	private readonly ISoftDelRepoMongo<Example2Id, Example2, DateTimeOffset> _repo;

	public Examples2Service(IDALContext context, IMapper mapper)
	{
		_mapper = mapper;
		_context = context;
		_repo = _context.Examples2;
	}

	public IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull => FilterConditionBuilder.GetFilters<Example2, TDto>();
	public IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>() where TDto : notnull => SortOrderConditionBuilder.GetSortOrders<Example2, TDto>();

	public async Task<long> GetCountAsync(SoftDel mode, FilterValues? filter)
	{
		var cond = filter.BuildCondition<Example2>();
		return await _repo.GetCountAsync(mode, cond);
	}

	public async Task<Example2Dto> GetAsync(Example2Id id)
	{
		var item = await _repo.GetAsync(id) ??
			throw new NotFound404Exception($"A Example2 [{id}] does not exist.");

		return _mapper.Map<Example2Dto>(item);
	}
	public async Task<IEnumerable<Example2Dto>> GetAsync(SoftDel mode, FilterValues? filter, SortOrderValues? sort, int pageSize, int pageNumber)
	{
		var items = await _repo.AsQueryable(mode)
			.Where(filter)
			.OrderBy(sort)
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

		return _mapper.Map<IEnumerable<Example2Dto>>(items);
	}

	public async Task<Example2Id> CreateAsync(Example2CreateDto dto)
	{
		var name = !string.IsNullOrWhiteSpace(dto.Id?.Name) ? dto.Id.Name.Trim() :
			throw new Conflict409Exception($"A Example2 name must be provided.");

		var code = dto.Id?.Code ?? 0;

		var desc = !string.IsNullOrWhiteSpace(dto.Desc) ? dto.Desc.Trim() : null;

		if (await _repo.GetCountAsync(SoftDel.All, x => x.Id == dto.Id) > 0)
			throw new Conflict409Exception($"A Example2 {name} already exists.");

		var item = _mapper.Map<Example2>(dto);
		item.Id = new Example2Id(name, code);
		item.Desc = desc;

		return (await _repo.CreateAsync(item)).Id!;
	}

	public async Task UpdateAsync(Example2Id id, Example2UpdateDto dto)
	{
		var item = await _repo.GetAsync(id) ??
			throw new NotFound404Exception($"A Example2 [{id.ToString()}] does not exist or has been deleted.");

		var desc = !string.IsNullOrWhiteSpace(dto.Desc) ? dto.Desc.Trim() : null;

		item.ForeignKey = dto.ForeignKey;
		item.Desc = desc;

		if (!await _repo.UpdateAsync(item))
			throw new NotFound404Exception();
	}

	public async Task DeleteAsync(Example2Id id)
	{
		if (!await _repo.DeleteAsync(id))
			throw new NotFound404Exception();
	}

	public async Task RestoreAsync(Example2Id id)
	{
		if (!await _repo.RestoreAsync(id))
			throw new NotFound404Exception();
	}
}