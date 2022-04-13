using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class UsersRepo : MongoSoftDelRepo<int, User, DateTimeOffset>
{
	public UsersRepo(IMongoDbContext context)
		: base(context, "users")
	{
	}
}