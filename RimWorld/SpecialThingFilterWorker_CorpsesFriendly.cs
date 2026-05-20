using Verse;

namespace RimWorld;

public class SpecialThingFilterWorker_CorpsesFriendly : SpecialThingFilterWorker
{
	public override bool Matches(Thing t)
	{
		if (!(t is Corpse corpse))
		{
			return false;
		}
		return corpse.InnerPawn.Faction == Faction.OfPlayer;
	}
}
