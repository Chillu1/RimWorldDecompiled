using Verse;

namespace RimWorld;

public class ThoughtWorker_Aurora : ThoughtWorker_GameCondition
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		Map mapHeld = p.MapHeld;
		if (mapHeld != null && mapHeld.GameConditionManager?.IsAlwaysDarkOutside == true)
		{
			return false;
		}
		return base.CurrentStateInternal(p).Active && p.SpawnedOrAnyParentSpawned && !p.PositionHeld.Roofed(p.MapHeld) && !PawnUtility.IsBiologicallyOrArtificiallyBlind(p);
	}
}
