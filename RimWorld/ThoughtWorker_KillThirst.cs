using Verse;

namespace RimWorld;

public class ThoughtWorker_KillThirst : ThoughtWorker
{
	public const float MinLevelForThought = 0.3f;

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		if (!ModsConfig.BiotechActive)
		{
			return ThoughtState.Inactive;
		}
		if (p.needs == null || !p.needs.TryGetNeed(out Need_KillThirst need))
		{
			return ThoughtState.Inactive;
		}
		return need.CurLevel <= 0.3f;
	}
}
