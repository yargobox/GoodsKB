namespace GoodsKB.DAL.Repositories;

/// <summary>
/// Repository custom identity provider interface
/// </summary>
public interface IIdentityGenerator<K>
{
	/// <summary>
	/// Generate a new Id
	/// </summary>
	K GenerateId(object container, object document);

	/// <summary>
	/// Generate a new Id asynchronously
	/// </summary>
	Task<K> GenerateIdAsync(object container, object document);

	/// <summary>
	/// Tests if id is empty.
	/// </summary>
	bool IsEmpty(K? id);

	/// <summary>
	/// The maximum number of attempts to insert with a new regenerated Id if it fails with DuplicateKey state on Id.
	/// This property is used in an optimistic approach to Id generation, where the generator tries the last known Id
	/// plus a step, or a pesemistic approach, where the generator obtains the last Id from the collection but does not
	/// block it.
	/// </summary>
	/// <remarks>
	/// The repository itself must support insert retries to implement both approaches.
	/// </remarks>
	int MaxAttempts { get; }
}