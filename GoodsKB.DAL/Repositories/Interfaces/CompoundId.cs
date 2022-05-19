namespace GoodsKB.DAL.Repositories;

/// <summary>
/// Interface of a complex identifier whose keys are stored together in a single field
/// </summary>
public interface ICompoundId : IEquatable<ICompoundId>
{
	abstract IEnumerable<object?> GetIdentityValues();
	abstract string SerializeToString();
	abstract byte[] SerializeToBinary();
}

/// <summary>
/// Basic implementation of a complex identifier whose keys are stored together in a single field
/// </summary>
public abstract class CompoundId<K, T> : IEquatable<CompoundId<K, T>>, ICompoundId
	where K : CompoundId<K, T>
{
	public static class Create
	{
		private static Func<string, K> _createFromString = null!;
		private static Func<byte[], K> _createFromBinary = null!;

		public static void MapCreator(Func<string, K> creator) => _createFromString = creator;
		public static void MapCreator(Func<byte[], K> creator) => _createFromBinary = creator;

		public static K FromStrng(string input)
		{
			if (_createFromString == null)
			{
				throw new InvalidOperationException($"{typeof(K).Name}.{nameof(Create)}.{nameof(MapCreator)}(string) must be called first.");
			}

			return _createFromString(input);
		}
		public static K FromBinary(byte[] input)
		{
			if (_createFromString == null)
			{
				throw new InvalidOperationException($"{typeof(K).Name}.{nameof(Create)}.{nameof(MapCreator)}(byte[]) must be called first.");
			}

			return _createFromBinary(input);
		}
	}

	public override bool Equals(object? obj)
	{
		if (object.ReferenceEquals(this, obj)) return true;
		if (object.ReferenceEquals(null, obj)) return false;
		var other = obj as CompoundId<K, T>;
		return other != null && GetIdentityValues().SequenceEqual(other.GetIdentityValues());
	}
	public virtual bool Equals(CompoundId<K, T>? other)
	{
		if (object.ReferenceEquals(this, other)) return true;
		if (object.ReferenceEquals(null, other)) return false;
		return GetIdentityValues().SequenceEqual(other.GetIdentityValues());
	}
	public virtual bool Equals(ICompoundId? obj)
	{
		if (object.ReferenceEquals(this, obj)) return true;
		if (object.ReferenceEquals(null, obj)) return false;
		var other = obj as CompoundId<K, T>;
		return other != null && GetIdentityValues().SequenceEqual(other.GetIdentityValues());
	}

	public override int GetHashCode() => GetIdentityValues().Select(x => x?.GetHashCode() ?? 0).Aggregate((a, b) => a ^ b);

	public override string ToString() => SerializeToString();

	public virtual string SerializeToString() => string.Join(':', GetIdentityValues());
	public virtual byte[] SerializeToBinary() => throw new NotSupportedException();

	public abstract IEnumerable<object?> GetIdentityValues();
}