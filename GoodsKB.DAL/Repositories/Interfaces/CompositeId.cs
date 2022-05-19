namespace GoodsKB.DAL.Repositories;

/// <summary>
/// Interface of a complex identifier whose keys are stored in separate fields
/// </summary>
public interface ICompositeId : IEquatable<ICompositeId>
{
	abstract IEnumerable<object?> GetIdentityValues();
	abstract string SerializeToString();
}

/// <summary>
/// Basic implementation of a complex identifier whose keys are stored in separate fields
/// </summary>
public abstract class CompositeId<K, T> : IEquatable<CompositeId<K, T>>, ICompositeId
	where K : CompositeId<K, T>
{
	public static class Create
	{
		private static Func<string, K> _create = null!;

		public static void MapCreator(Func<string, K> creator) => _create = creator;

		public static K FromStrng(string fromString)
		{
			if (_create == null)
			{
				throw new InvalidOperationException($"{typeof(K).Name}.{nameof(Create)}.{nameof(MapCreator)} must be called first.");
			}

			return _create(fromString);
		}
	}

	public override bool Equals(object? obj)
	{
		if (object.ReferenceEquals(this, obj)) return true;
		if (object.ReferenceEquals(null, obj)) return false;
		var other = obj as CompositeId<K, T>;
		return other != null && GetIdentityValues().SequenceEqual(other.GetIdentityValues());
	}
	public virtual bool Equals(CompositeId<K, T>? other)
	{
		if (object.ReferenceEquals(this, other)) return true;
		if (object.ReferenceEquals(null, other)) return false;
		return GetIdentityValues().SequenceEqual(other.GetIdentityValues());
	}
	public virtual bool Equals(ICompositeId? obj)
	{
		if (object.ReferenceEquals(this, obj)) return true;
		if (object.ReferenceEquals(null, obj)) return false;
		var other = obj as CompositeId<K, T>;
		return other != null && GetIdentityValues().SequenceEqual(other.GetIdentityValues());
	}

	public override int GetHashCode() => GetIdentityValues().Select(x => x?.GetHashCode() ?? 0).Aggregate((a, b) => a ^ b);

	public override string ToString() => SerializeToString();

	public virtual string SerializeToString() => string.Join(':', GetIdentityValues());

	public abstract IEnumerable<object?> GetIdentityValues();
}