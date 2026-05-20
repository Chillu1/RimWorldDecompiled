using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_CorpsesColonist : SpecialThingFilterWorker
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
		if (ModsConfig.IdeologyActive && corpse.InnerPawn.IsSlave)
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && t is UnnaturalCorpse)
		{
			return false;
		}
		return corpse.InnerPawn.Faction == Faction.OfPlayer;
	}
}
