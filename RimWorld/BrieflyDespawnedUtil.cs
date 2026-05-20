using Verse;

namespace RimWorld;

public static class BrieflyDespawnedUtil
{
	public static bool BrieflyDespawned(this Pawn pawn)
	{
		if (pawn.Spawned)
		{
			return false;
		}
		if (pawn.ParentHolder is PawnFlyer)
		{
			return true;
		}
		return false;
	}
}
