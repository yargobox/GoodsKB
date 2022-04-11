using MongoDB.Driver;

namespace GoodsKB.DAL.Configuration;

public interface IMongoDbContext
{
	MongoClient Client { get; }
	IMongoDatabase DB { get; }
	IMongoCollection<T> GetCollection<T>(string name);
}

public class MongoDbContext : IMongoDbContext
{
	public MongoClient Client { get; }
	public IMongoDatabase DB { get; }

	public MongoDbContext(MongoDbSettings mongoDbConfig)
	{
		Client = new MongoClient(mongoDbConfig.ConnectionString);
		DB = Client.GetDatabase(mongoDbConfig.Catalog);
	}

	public IMongoCollection<T> GetCollection<T>(string name)
	{
		return DB.GetCollection<T>(name);
	}
}