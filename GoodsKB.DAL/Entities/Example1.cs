namespace GoodsKB.DAL.Entities;

using GoodsKB.DAL.Repositories;
using GoodsKB.DAL.Runtime;

public sealed class Example1Id : CompositeId<Example1Id, Example1>
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
			if (string.IsNullOrEmpty(s))
			{
				return new Example1Id();
			}

			var i = s.LastIndexOf(':');
			if (i < 0)
			{
				throw new ArgumentException($"{nameof(Example1Id)}.{nameof(Create)}.{nameof(Create.FromStrng)}({s})");
			}

			string? name = i > 0 ? s.Substring(0, i) : null;

			int parsedCode;
			int? code = i + 1 < s.Length && int.TryParse(s.Substring(i + 1), out parsedCode) ? parsedCode : null;

			return new Example1Id(name, code);
		});
	}
}

public class Example1 : IUpdatedEntity<Example1Id, DateTimeOffset>, ISoftDelEntity<DateTimeOffset>
{
	public Example1Id? Id { get; set; }

	public string? Desc { get; set; }

	public DateTimeOffset? Created { get; set; }
	public DateTimeOffset? Updated { get; set; }
	public DateTimeOffset? Deleted { get; set; }
}