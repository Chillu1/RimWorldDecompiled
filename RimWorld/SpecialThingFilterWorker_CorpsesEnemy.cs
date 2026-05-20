using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_CorpsesEnemy : SpecialThingFilterWorker
{
	public override bool Matches(Thing t)
	{
		if (!(t is Corpse corpse))
		{
			return false;
		}
		if (corpse.InnerPawn.Faction != null)
		{
			return corpse.InnerPawn.Faction.HostileTo(Faction.OfPlayer);
		}
		return false;
	}
}
