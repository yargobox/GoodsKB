namespace GoodsKB.API.Mapping;

using AutoMapper;
using GoodsKB.API.Models;
using GoodsKB.BLL.DTOs;
using GoodsKB.DAL.Entities;

internal class Example1Mapping : Profile
{
	public Example1Mapping()
	{
		// Controller
		CreateMap<Example1Dto, Example1Model>();
		CreateMap<Example1CreateModel, Example1CreateDto>();
		CreateMap<Example1UpdateModel, Example1UpdateDto>();

		// Service
		CreateMap<Example1, Example1Dto>();
		CreateMap<Example1CreateDto, Example1>();
		//CreateMap<Example1UpdateDto, Example1>();
	}
}