namespace GoodsKB.DAL.Entities;

using GoodsKB.DAL.Repositories;
using GoodsKB.DAL.Runtime;
using MongoDB.Bson.Serialization;

public sealed class Example1Id : CompoundId<Example1Id, Example1>
{
	public string? Name { get; set; }
	public int? Code { get; set; }

	public Example1Id() { }

	public Example1Id(string? name, int? code)
	{
		Name = name;
		Code = code;
	}

	public override IEnumerable<object?> GetIdentityValues() => new object?[] { Name, Code };

	[EagerLoading]
	static Example1Id()
	{
		Create.MapCreator(s =>
		{
			int code;
			var i = s.LastIndexOf(':');
			
			if (i < 0 || i + 1 >= s.Length || !int.TryParse(s.Substring(i + 1), out code))
			{
				throw new ArgumentException($"{nameof(Example1Id)}.{nameof(Create)}.{nameof(Create.FromStrng)}({s})");
			}

			return new Example1Id(s.Substring(0, i), code);
		});

		BsonClassMap.RegisterClassMap<Example1Id>(cm =>
		{
			cm.MapProperty(c => c.Name);
			cm.MapProperty(c => c.Code);
			cm.MapCreator(c => new Example1Id(c.Name, c.Code));
		});
	}
}

public class Example1 : IEntity<Example1Id>, ISoftDelEntity<DateTimeOffset>
{
	public Example1() { }

	public Example1(Example1Id? id, string? desc, DateTimeOffset? created, DateTimeOffset? updated, DateTimeOffset? deleted)
	{
		Id = id;
		Desc = desc;
		Created = created;
		Updated = updated;
		Deleted = deleted;
	}

	public Example1Id? Id { get; set; }

	public string? Desc { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }

	[EagerLoading]
	static Example1()
	{
		BsonClassMap.RegisterClassMap<Example1>(cm =>
		{
			cm.AutoMap();
			cm.MapIdMember(c => c.Id);
			cm.MapCreator(c => new Example1(c.Id, c.Desc, c.Created, c.Updated, c.Deleted));
		});
	}
}