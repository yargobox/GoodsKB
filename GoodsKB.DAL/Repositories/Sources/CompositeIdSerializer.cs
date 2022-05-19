namespace GoodsKB.DAL.Repositories;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

public class CompositeIdSerializer<K> : IBsonSerializer<K?> where K : class?
{
	private static Func<string, K>? _create;

	public void MapCreator(Func<string, K> creator) => _create = creator;

	public CompositeIdSerializer() { }
	public CompositeIdSerializer(BsonType representation)
	{
		if (representation != BsonType.String)
		{
			throw new ArgumentException($"{representation.ToString()} is not a valid representation for a {nameof(K)}.");
		}
	}
	public CompositeIdSerializer(BsonType representation, Func<string, K> creator)
	{
		if (representation != BsonType.String)
		{
			throw new ArgumentException($"{representation.ToString()} is not a valid representation for a {nameof(K)}.");
		}

		_create = creator;
	}

	public Type ValueType => typeof(K);

	public K? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
	{
		var state = context.Reader.State;
		if (state == MongoDB.Bson.IO.BsonReaderState.Value)
		{
			var type = context.Reader.GetCurrentBsonType();
			if (type == BsonType.String)
			{
				if (_create == null)
				{
					throw new InvalidOperationException($"{nameof(CompositeIdSerializer<K>)}<{typeof(K).Name}>.{nameof(MapCreator)} must be called first.");
				}

				var s = context.Reader.ReadString();
				return _create(s);
			}
			else if (type == BsonType.Null)
			{
				context.Reader.ReadNull();
				return null;
			}
			else
			{
				throw new BsonSerializationException($"'{type.ToString()}' is not a valid {nameof(K)} representation.");
			}
		}
		else
		{
			throw new BsonSerializationException($"Invalid state of the reader in {nameof(CompositeIdSerializer<K>)}<{typeof(K).Name}>.");
		}
	}

	public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, K? value)
	{
		if (value == null)
			context.Writer.WriteNull();
		else
			context.Writer.WriteString(value.ToString());
	}

	public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
	{
		if (value == null)
			context.Writer.WriteNull();
		else
			context.Writer.WriteString(value.ToString());
	}

	object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
	{
		return Deserialize(context, args)!;
	}
}