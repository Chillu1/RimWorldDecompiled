using Verse;

namespace RimWorld
{
	public interface IHaulDestination : IStoreSettingsParent
	{
		IntVec3 Position
		{
			get;
		}

		Map Map
		{
			get;
		}

		bool Accepts(Thing t);
	}
}
