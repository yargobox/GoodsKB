namespace GoodsKB.DAL.Repositories;

internal sealed class GuidIdentityProvider : IIdentityProvider<Guid>
{
	public Task LoadAsync()
	{
		return Task.CompletedTask;
	}
	public bool ShouldProvideNextIdentity(Guid? id)
	{
		return id == null || id == Guid.Empty;
	}
	public bool ShouldProvideNextIdentity(Guid id)
	{
		return id == Guid.Empty;
	}
	public Guid NextIdentity()
	{
		return Guid.NewGuid();
	}
	public async Task<Guid> NextIdentityAsync()
	{
		return await Task.FromResult(Guid.NewGuid());
	}
}