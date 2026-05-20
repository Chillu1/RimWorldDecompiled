using Verse;

namespace RimWorld;

public class ThoughtWorker_SwallowedByDarkness : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		return ModsConfig.AnomalyActive && InDarkness(p) && p.MapHeld.GameConditionManager.MapBrightness < 0.5f;
	}

	public static bool InDarkness(Pawn pawn)
	{
		if (!pawn.SpawnedOrAnyParentSpawned || pawn.Dead || pawn.Suspended)
		{
			return false;
		}
		return pawn.MapHeld.glowGrid.GroundGlowAt(pawn.PositionHeld) <= 0f;
	}
}
