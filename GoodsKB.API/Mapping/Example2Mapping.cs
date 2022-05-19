namespace GoodsKB.API.Mapping;

using AutoMapper;
using GoodsKB.API.Models;
using GoodsKB.BLL.DTOs;
using GoodsKB.DAL.Entities;

internal class Example2Mapping : Profile
{
	public Example2Mapping()
	{
		// Controller
		CreateMap<Example2Dto, Example2Model>();
		CreateMap<Example2CreateModel, Example2CreateDto>();
		CreateMap<Example2UpdateModel, Example2UpdateDto>();

		// Service
		CreateMap<Example2, Example2Dto>();
		CreateMap<Example2CreateDto, Example2>();
		//CreateMap<Example2UpdateDto, Example2>();
	}
}