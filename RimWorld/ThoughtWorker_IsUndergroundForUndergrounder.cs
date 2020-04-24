using Verse;

namespace RimWorld
{
	public class ThoughtWorker_IsUndergroundForUndergrounder : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			bool isNaturalRoof;
			return ThoughtWorker_IsIndoorsForUndergrounder.IsAwakeAndIndoors(p, out isNaturalRoof) && isNaturalRoof;
		}
	}
}
