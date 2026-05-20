using Verse;

namespace RimWorld;

public class ThoughtWorker_Precept_NoFish : ThoughtWorker_Precept
{
	protected override ThoughtState ShouldHaveThought(Pawn p)
	{
		if (!ModsConfig.OdysseyActive || !p.SpawnedOrAnyParentSpawned)
		{
			return ThoughtState.Inactive;
		}
		return !p.MapHeld.waterBodyTracker.AnyBodyContainsFish;
	}
}
