using GoodsKB.API.Models;
using GoodsKB.BLL.DTOs;
using GoodsKB.DAL.Entities;
using AutoMapper;

namespace GoodsKB.API.Mapping;

public class UserMapping : Profile
{
	public UserMapping()
	{
		// Model => DTO
		CreateMap<UserModel, UserDto>();
		CreateMap<UserCreateModel, UserCreateDto>();
		CreateMap<UserUpdateModel, UserUpdateDto>();
		// DTO => Model
		CreateMap<UserDto, UserModel>();
		// DTO => Entity
		CreateMap<UserDto, User>();
		CreateMap<UserCreateDto, User>();
		CreateMap<UserUpdateDto, User>();
		// Entity => DTO
		CreateMap<User, UserDto>();
	}
}