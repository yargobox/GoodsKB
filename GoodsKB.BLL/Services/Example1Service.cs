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

public interface IExamples1Service
{
	IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull;
	IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>() where TDto : notnull;
	Task<long> GetCountAsync(SoftDel mode, FilterValues? filter);
	Task<IEnumerable<Example1Dto>> GetAsync(SoftDel mode, FilterValues? filter, SortOrderValues? sort, int pageSize, int pageNumber);
	Task<Example1Dto> GetAsync(Example1Id id);
	Task<Example1Id> CreateAsync(Example1CreateDto dto);
	Task UpdateAsync(Example1Id id, Example1UpdateDto dto);
	Task DeleteAsync(Example1Id id);
	Task RestoreAsync(Example1Id id);
}

public class Examples1Service : IExamples1Service
{
	private readonly IMapper _mapper;
	private readonly IDALContext _context;
	private readonly ISoftDelRepoMongo<Example1Id, Example1, DateTimeOffset> _repo;

	public Examples1Service(IDALContext context, IMapper mapper)
	{
		_mapper = mapper;
		_context = context;
		_repo = _context.Examples1;
	}

	public IReadOnlyDictionary<string, FilterDesc> GetFilters<TDto>() where TDto : notnull => FilterConditionBuilder.GetFilters<Example1, TDto>();
	public IReadOnlyDictionary<string, SortOrderDesc> GetSortOrders<TDto>() where TDto : notnull => SortOrderConditionBuilder.GetSortOrders<Example1, TDto>();

	public async Task<long> GetCountAsync(SoftDel mode, FilterValues? filter)
	{
		var cond = filter.BuildCondition<Example1>();
		return await _repo.GetCountAsync(mode, cond);
	}

	public async Task<Example1Dto> GetAsync(Example1Id id)
	{
		var item = await _repo.GetAsync(id) ??
			throw new NotFound404Exception($"A Example1 [{id}] does not exist.");

		return _mapper.Map<Example1Dto>(item);
	}
	public async Task<IEnumerable<Example1Dto>> GetAsync(SoftDel mode, FilterValues? filter, SortOrderValues? sort, int pageSize, int pageNumber)
	{
		var items = await _repo.GetQuery(mode)
			.Where(filter)
			.OrderBy(sort)
			.Skip((pageNumber - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync();

		return _mapper.Map<IEnumerable<Example1Dto>>(items);
	}

	public async Task<Example1Id> CreateAsync(Example1CreateDto dto)
	{
		var name = !string.IsNullOrWhiteSpace(dto.Id?.Name) ? dto.Id.Name.Trim() :
			throw new Conflict409Exception($"A Example1 name must be provided.");

		var code = dto.Id?.Code ?? 0;

		var desc = !string.IsNullOrWhiteSpace(dto.Desc) ? dto.Desc.Trim() : null;

		if (await _repo.GetCountAsync(SoftDel.All, x => x.Id!.Name!.ToLower() == name.ToLower() && x!.Id.Code == code) > 0)
			throw new Conflict409Exception($"A Example1 {name} already exists.");

		var item = _mapper.Map<Example1>(dto);
		item.Id = new Example1Id(name, code);
		item.Desc = desc;

		return (await _repo.CreateAsync(item)).Id!;
	}

	public async Task UpdateAsync(Example1Id id, Example1UpdateDto dto)
	{
		var item = await _repo.GetAsync(id) ??
			throw new NotFound404Exception($"A Example1 [{id.ToString()}] does not exist or has been deleted.");

		var desc = !string.IsNullOrWhiteSpace(dto.Desc) ? dto.Desc.Trim() : null;

		item.Desc = desc;

		if (!await _repo.UpdateAsync(item))
			throw new NotFound404Exception();
	}

	public async Task DeleteAsync(Example1Id id)
	{
		if (!await _repo.DeleteAsync(id))
			throw new NotFound404Exception();
	}

	public async Task RestoreAsync(Example1Id id)
	{
		if (!await _repo.RestoreAsync(id))
			throw new NotFound404Exception();
	}
}