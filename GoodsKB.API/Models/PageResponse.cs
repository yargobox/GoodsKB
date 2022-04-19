using System.Collections;
using Microsoft.AspNetCore.Mvc;

namespace GoodsKB.API.Models;

public sealed class PagedResponse<T>
	where T : IEnumerable
{
	public int PageSize { get; set; }
	public int PageNumber { get; set; }
	public int TotalRows { get; set; }
	public int TotalPages { get; set; }
	public T Data { get; set; }

	public PagedResponse(T data, int pageSize, int pageNumber, int totalRows)
	{
		Data = data;
		PageSize = pageSize;
		PageNumber = pageNumber;
		TotalRows = totalRows;
		TotalPages = (int) Math.Ceiling((double) totalRows / pageSize);
	}
}