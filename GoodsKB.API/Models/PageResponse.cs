using System.Collections;
using Microsoft.AspNetCore.Mvc;

namespace GoodsKB.API.Models;

internal class PagedResponse<T>
{
	public int PageSize { get; set; }
	public int PageNumber { get; set; }
	public long TotalRows { get; set; }
	public long TotalPages => (long)Math.Ceiling((double)TotalRows / PageSize);
	public IEnumerable<T> Data { get; set; }

	public PagedResponse(IEnumerable<T> data, int pageSize, int pageNumber, long totalRows)
	{
		Data = data;
		PageSize = pageSize;
		PageNumber = pageNumber;
		TotalRows = totalRows;
	}
}