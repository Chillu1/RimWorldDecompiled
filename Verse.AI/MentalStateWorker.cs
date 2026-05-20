using RimWorld;
using RimWorld.Planet;

namespace Verse.AI
{
	public class MentalStateWorker
	{
		public MentalStateDef def;

		public virtual bool StateCanOccur(Pawn pawn)
		{
			if (!def.inCaravanCanDo && !pawn.Spawned && pawn.IsCaravanMember())
			{
				return false;
			}
			if (!def.unspawnedNotInCaravanCanDo && !pawn.Spawned && !pawn.IsCaravanMember())
			{
				return false;
			}
			if (!def.prisonersCanDo && pawn.IsPrisoner)
			{
				return false;
			}
			if (!def.slavesCanDo && pawn.IsSlave)
			{
				return false;
			}
			if (def.colonistsOnly && pawn.Faction != Faction.OfPlayer)
			{
				return false;
			}
			if (def.slavesOnly && !pawn.IsSlave)
			{
				return false;
			}
			if (!def.downedCanDo && pawn.Downed)
			{
				return false;
			}
			if (pawn.IsWorldPawn())
			{
				return false;
			}
			for (int i = 0; i < def.requiredCapacities.Count; i++)
			{
				if (!pawn.health.capacities.CapableOf(def.requiredCapacities[i]))
				{
					return false;
				}
			}
			return true;
		}
	}
}
