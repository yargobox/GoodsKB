using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;
using GoodsKB.DAL.Repositories.Mongo;

namespace GoodsKB.DAL.Repositories;

internal class UsersRepo : MongoSoftDelRepo<int, User, DateTimeOffset>
{
	public UsersRepo(IMongoDbContext context)
		: base(context, "users",
			new SequenceIdentityProvider<int>(context, "users", 0, 1, x => x == 0)
		)
	{
	}

	protected override async void OnLoad()
	{
		base.OnLoad();

		var indexeNames = await GetIndexNames();

		var indexName = "username_ux";
		if (!indexeNames.Contains(indexName)) await CreateIndex(indexName, true, x => x.Username);

		indexName = "email_ux";
		if (!indexeNames.Contains(indexName)) await CreateIndex(indexName, true, x => x.Email, false, null, x => x.Email != null);

		indexName = "phone_ux";
		if (!indexeNames.Contains(indexName)) await CreateIndex(indexName, true, x => x.Phone, false, null, x => x.Phone != null);
	}
}

internal class TestRec : IIdentifiableEntity<string>, ISoftDelEntity<DateTimeOffset>
{
	public string Id { get; set; } = string.Empty;
	public DateTimeOffset? Deleted { get; set; }
}

internal class TestRepo : MongoSoftDelRepo<string, TestRec, DateTimeOffset>
{
	public TestRepo(IMongoDbContext context)
		: base(context, "tests")
	{
	}
}