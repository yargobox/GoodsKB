namespace GoodsKB.DAL.Repositories;

public interface IEntity<K, TDateTime>
{
	K? Id { get; set; }
}

public interface ICreatedEntity<K, TDateTime> : IEntity<K, TDateTime> where TDateTime : struct
{
	TDateTime? Created { get; set; }
}

public interface IUpdatedEntity<K, TDateTime> : ICreatedEntity<K, TDateTime> where TDateTime : struct
{
	TDateTime? Updated { get; set; }
}

public interface IModifiedEntity<K, TDateTime> : IEntity<K, TDateTime> where TDateTime : struct
{
	TDateTime? Modified { get; set; }
}

public interface ISoftDelEntity<TDateTime> where TDateTime : struct
{
	TDateTime? Deleted { get; set; }
}