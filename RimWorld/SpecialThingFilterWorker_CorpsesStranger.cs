using Verse;

namespace RimWorld
{
	public class SpecialThingFilterWorker_CorpsesStranger : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			Corpse corpse = t as Corpse;
			if (corpse == null)
			{
				return false;
			}
			if (!corpse.InnerPawn.def.race.Humanlike)
			{
				return false;
			}
			return corpse.InnerPawn.Faction != Faction.OfPlayer;
		}
	}
}
