using RimWorld;

namespace Verse.AI
{
	public class MentalStateWorker
	{
		public MentalStateDef def;

		public virtual bool StateCanOccur(Pawn pawn)
		{
			if (!def.unspawnedCanDo && !pawn.Spawned)
			{
				return false;
			}
			if (!def.prisonersCanDo && pawn.HostFaction != null)
			{
				return false;
			}
			if (def.colonistsOnly && pawn.Faction != Faction.OfPlayer)
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
