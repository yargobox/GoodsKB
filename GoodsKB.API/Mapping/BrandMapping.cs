namespace GoodsKB.API.Mapping;

using AutoMapper;
using GoodsKB.API.Models;
using GoodsKB.BLL.DTOs;
using GoodsKB.DAL.Entities;

internal class BrandMapping : Profile
{
	public BrandMapping()
	{
		// Controller
		CreateMap<BrandDto, BrandModel>();
		CreateMap<BrandCreateModel, BrandCreateDto>();
		CreateMap<BrandUpdateModel, BrandUpdateDto>();

		// Service
		CreateMap<Brand, BrandDto>();
		CreateMap<BrandCreateDto, Brand>();
		//CreateMap<BrandUpdateDto, Brand>();
	}
}