namespace GoodsKB.DAL.Repositories;

public interface IIdentityProvider<TKey>
{
	Task LoadAsync();
	bool ShouldProvideNextIdentity(TKey? id);
	Task<TKey> NextIdentityAsync();
}