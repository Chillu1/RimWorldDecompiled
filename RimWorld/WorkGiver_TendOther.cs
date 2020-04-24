using Verse;

namespace RimWorld
{
	public class WorkGiver_TendOther : WorkGiver_Tend
	{
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (base.HasJobOnThing(pawn, t, forced))
			{
				return pawn != t;
			}
			return false;
		}
	}
}
