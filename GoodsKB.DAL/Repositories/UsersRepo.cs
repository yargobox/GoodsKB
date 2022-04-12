using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class UsersRepo : MongoIntIdentitySoftDelRepo<User>
{
	public UsersRepo(IMongoDbContext context)
		: base(context, "users")
	{
	}
}