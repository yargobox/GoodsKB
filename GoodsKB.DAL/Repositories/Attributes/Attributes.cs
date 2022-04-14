using GoodsKB.DAL.Common;

namespace GoodsKB.DAL.Repositories;

public enum RepositoryIdentityTypes
{
	Default = 0,
	Random = 1,
	Sequential = 2,
	Complex = 3
}

public enum RepositoryIdentityProviders
{
	Driver = 0,
	Repository = 1,
	User = 2
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited=false)]
public class RepositoryIdentityAttribute : Attribute
{
	public static readonly DefaultIdentityAttribute DefaultIdentity = new DefaultIdentityAttribute();
	public RepositoryIdentityTypes IdentityType { get; }
	public RepositoryIdentityProviders IdentityProvider { get; }

	public RepositoryIdentityAttribute(RepositoryIdentityTypes identityType, RepositoryIdentityProviders identityProvider)
	{
		IdentityType = identityType;
		IdentityProvider = identityProvider;
	}
}
public class DefaultIdentityAttribute : RepositoryIdentityAttribute
{
	public DefaultIdentityAttribute() : base(RepositoryIdentityTypes.Default, RepositoryIdentityProviders.Driver) { }
}
public class RandomIdentityAttribute : RepositoryIdentityAttribute
{
	public RandomIdentityAttribute() : base(RepositoryIdentityTypes.Random, RepositoryIdentityProviders.Repository) { }
}
public class SequentialIdentityAttribute : RepositoryIdentityAttribute
{
	public SequentialIdentityAttribute() : base(RepositoryIdentityTypes.Sequential, RepositoryIdentityProviders.Repository) { }
}
public class ComplexIdentityAttribute : RepositoryIdentityAttribute
{
	public ComplexIdentityAttribute() : base(RepositoryIdentityTypes.Complex, RepositoryIdentityProviders.User) { }
}

public static class RepositoryAttributeExtensions
{
	public static (RepositoryIdentityTypes identityType, RepositoryIdentityProviders identityProvider)
		GetIdentityInfo<TKey, TEntity>(this IRepoBase<TKey, TEntity> repo)
			where TKey : struct
			where TEntity : IIdentifiableEntity<TKey>
	{
		var attr = repo.GetType().GetAttribute<RepositoryIdentityAttribute>();
		if (attr != null)
			return (attr.IdentityType, attr.IdentityProvider);
		else
			return (RepositoryIdentityAttribute.DefaultIdentity.IdentityType, RepositoryIdentityAttribute.DefaultIdentity.IdentityProvider);
	}
}