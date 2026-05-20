namespace RimWorld
{
	public interface IWaitForItemsLordToil
	{
		int CountRemaining { get; }

		bool HasAllRequestedItems { get; }
	}
}
