using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AutoMapper;
using GoodsKB.API.Models;
using GoodsKB.BLL.DTOs;
using GoodsKB.DAL.Entities;

namespace GoodsKB.API.Mapping;

public class UserMapping : Profile
{
	public UserMapping()
	{
		/* 		map.AutoMap();
				map.SetIgnoreExtraElements(true);
				map.MapMember(x => x.Gender).SetSerializer(new NullableSerializer<Gender>(new EnumSerializer<Gender>(BsonType.String))); */

		/* 		if (!MongoDB.Bson.Serialization.BsonClassMap.IsClassMapRegistered(typeof(UserModel)))
				{
					MongoDB.Bson.Serialization.BsonClassMap.RegisterClassMap<UserModel>(cm =>
						{
							cm.AutoMap();
							cm.GetMemberMap(c => c.Roles).SetRepresentation(BsonType.String);
						});
				} */

		//CreateMap<string, UserRoles>().ConvertUsing<StringToEnumConverter<UserRoles>>();
		//CreateMap<UserRoles, string>().ConvertUsing<EnumToStringConverter<UserRoles>>();

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

	public sealed class EnumToStringConverter<T> : ITypeConverter<T, string>  where T : struct
	{
		public string Convert(T source, string destination, ResolutionContext context)
		{
			return source.ToString()!;
		}
	}
	public sealed class StringToEnumConverter<T> : ITypeConverter<string, T>, ITypeConverter<string, T?> where T : struct
	{
		public T Convert(string source, T destination, ResolutionContext context)
		{
			T t;
			if (Enum.TryParse(source, out t))
			{
				return t;
			}
			throw new FormatException();
		}

		T? ITypeConverter<string, T?>.Convert(string source, T? destination, ResolutionContext context)
		{
			if (source == null) return null;
			return Convert(source, default(T), context);
		}
	}
}