using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_CorpsesSlave : SpecialThingFilterWorker
{
	public override bool Matches(Thing t)
	{
		if (!(t is Corpse corpse))
		{
			return false;
		}
		if (!corpse.InnerPawn.def.race.Humanlike)
		{
			return false;
		}
		if (corpse.InnerPawn.Faction == Faction.OfPlayer)
		{
			return corpse.InnerPawn.IsSlave;
		}
		return false;
	}
}
