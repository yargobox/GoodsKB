namespace GoodsKB.DAL.Entities;

using GoodsKB.DAL.Repositories;
using GoodsKB.DAL.Runtime;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

public sealed class Example2Id : CompoundId<Example2Id, Example2>
{
	public string? Name { get; set; }
	public int? Code { get; set; }

	public Example2Id() { }

	public Example2Id(string? name, int? code)
	{
		Name = name;
		Code = code;
	}

	public override IEnumerable<object?> GetIdentityValues() => new object?[] { Name, Code };

	[EagerLoading]
	static Example2Id()
	{
		Create.MapCreator(s =>
		{
			if (string.IsNullOrEmpty(s))
			{
				return new Example2Id();
			}

			var i = s.LastIndexOf(':');
			if (i < 0)
			{
				throw new ArgumentException($"{nameof(Example2Id)}.{nameof(Create)}.{nameof(Create.FromStrng)}({s})");
			}

			string? name = i > 0 ? s.Substring(0, i) : null;

			int parsedCode;
			int? code = i + 1 < s.Length && int.TryParse(s.Substring(i + 1), out parsedCode) ? parsedCode : null;

			return new Example2Id(name, code);
		});

		// CompoundId has a string serializer
		BsonSerializer.RegisterSerializer(new CompoundIdSerializer<Example2Id, Example2>(BsonType.String));
	}
}

public class Example2 : IUpdatedEntity<Example2Id, DateTimeOffset>, ISoftDelEntity<DateTimeOffset>
{
	public Example2Id? Id { get; set; }

	public Example2Id? ForeignKey { get; set; }

	public string? Desc { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}