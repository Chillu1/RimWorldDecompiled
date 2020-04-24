using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver
	{
		public WorkGiverDef def;

		public virtual bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return false;
		}

		public virtual Job NonScanJob(Pawn pawn)
		{
			return null;
		}

		public PawnCapacityDef MissingRequiredCapacity(Pawn pawn)
		{
			for (int i = 0; i < def.requiredCapacities.Count; i++)
			{
				if (!pawn.health.capacities.CapableOf(def.requiredCapacities[i]))
				{
					return def.requiredCapacities[i];
				}
			}
			return null;
		}
	}
}
