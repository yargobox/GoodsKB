namespace GoodsKB.DAL.Repositories;

public interface ICompoundId : IEquatable<ICompoundId>
{
	abstract IEnumerable<object?> GetIdentityValues();
	abstract string SerializeToString();
}

public abstract class CompoundId<K, T> : IEquatable<CompoundId<K, T>>, ICompoundId where K : CompoundId<K, T>, new()
{
	public static class Create
	{
		private static volatile Func<string, K> _create = null!;

		public static void MapCreator(Func<string, K> creator) => _create = creator;

		public static K FromStrng(string fromString) => (_create ?? throw new InvalidOperationException($"{typeof(K).Name}")) (fromString);
	}

	public static readonly K Empty = new K();

	public override bool Equals(object? obj)
	{
		if (object.ReferenceEquals(this, obj)) return true;
		if (object.ReferenceEquals(null, obj)) return false;
		var other = obj as CompoundId<K, T>;
		return other != null && GetIdentityValues().SequenceEqual(other.GetIdentityValues());
	}
	public bool Equals(CompoundId<K, T>? other)
	{
		if (object.ReferenceEquals(this, other)) return true;
		if (object.ReferenceEquals(null, other)) return false;
		return GetIdentityValues().SequenceEqual(other.GetIdentityValues());
	}
	public bool Equals(ICompoundId? obj)
	{
		if (object.ReferenceEquals(this, obj)) return true;
		if (object.ReferenceEquals(null, obj)) return false;
		var other = obj as CompoundId<K, T>;
		return other != null && GetIdentityValues().SequenceEqual(other.GetIdentityValues());
	}

	public override int GetHashCode() => GetIdentityValues().Select(x => x?.GetHashCode() ?? 0).Aggregate((a, b) => a ^ b);

	public override string ToString() => SerializeToString();

	public virtual string SerializeToString() => string.Join(':', GetIdentityValues());

	public abstract IEnumerable<object?> GetIdentityValues();
}