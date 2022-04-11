namespace GoodsKB.BLL.Exceptions;

public class NotFound404Exception : ApplicationException
{
	public NotFound404Exception()
	{
	}

	public NotFound404Exception(string errorMessage) : base(errorMessage)
	{
	}
}