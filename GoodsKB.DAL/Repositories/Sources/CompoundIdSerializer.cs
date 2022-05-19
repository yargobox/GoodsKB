namespace GoodsKB.DAL.Repositories;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

public class CompoundIdSerializer<K, T> : IBsonSerializer<K?>
	where K : CompoundId<K, T>
{
	public CompoundIdSerializer(BsonType representation)
	{
		if (representation != BsonType.String)
		{
			throw new ArgumentException($"{representation.ToString()} is not a valid representation for a {nameof(K)}.");
		}
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
				var s = context.Reader.ReadString();
				return CompoundId<K, T>.Create.FromStrng(s);
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
			throw new BsonSerializationException($"Invalid state of the reader in {nameof(CompoundIdSerializer<K, T>)}<{typeof(K).Name}>.");
		}
	}

	public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, K? value)
	{
		if (value == null)
			context.Writer.WriteNull();
		else
			context.Writer.WriteString(value.SerializeToString());
	}

	public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
	{
		if (value == null)
			context.Writer.WriteNull();
		else
			context.Writer.WriteString(((K)value).SerializeToString());
	}

	object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
	{
		return Deserialize(context, args)!;
	}
}