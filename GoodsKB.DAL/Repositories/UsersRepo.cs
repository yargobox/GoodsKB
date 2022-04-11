using GoodsKB.DAL.Configuration;
using GoodsKB.DAL.Entities;

namespace GoodsKB.DAL.Repositories;

internal class UsersRepo : MongoIntIdentityRepo<User>
{
	public UsersRepo(IMongoDbContext context)
		: base(context, "users")
	{
	}
}