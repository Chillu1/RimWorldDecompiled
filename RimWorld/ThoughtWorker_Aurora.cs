using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Aurora : ThoughtWorker_GameCondition
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			return base.CurrentStateInternal(p).Active && p.SpawnedOrAnyParentSpawned && !p.PositionHeld.Roofed(p.MapHeld) && p.health.capacities.CapableOf(PawnCapacityDefOf.Sight);
		}
	}
}
