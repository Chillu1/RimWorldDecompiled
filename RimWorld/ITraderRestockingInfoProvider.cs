namespace RimWorld
{
	public interface ITraderRestockingInfoProvider
	{
		bool EverVisited
		{
			get;
		}

		bool RestockedSinceLastVisit
		{
			get;
		}

		int NextRestockTick
		{
			get;
		}
	}
}
