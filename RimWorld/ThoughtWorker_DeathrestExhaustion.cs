using Verse;

namespace RimWorld;

public class ThoughtWorker_DeathrestExhaustion : ThoughtWorker
{
	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		if (p.needs == null || !p.needs.TryGetNeed(out Need_Deathrest need))
		{
			return ThoughtState.Inactive;
		}
		return need.CurLevel == 0f;
	}
}
