using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_ApparelDamaged : ThoughtWorker
{
	public const float MinForFrayed = 0.5f;

	public const float MinForTattered = 0.2f;

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		float num = 999f;
		List<Apparel> wornApparel = p.apparel.WornApparel;
		for (int i = 0; i < wornApparel.Count; i++)
		{
			if (wornApparel[i].def.useHitPoints && !p.apparel.IsLocked(wornApparel[i]) && wornApparel[i].def.apparel.careIfDamaged)
			{
				float num2 = (float)wornApparel[i].HitPoints / (float)wornApparel[i].MaxHitPoints;
				if (num2 < num)
				{
					num = num2;
				}
				if (num < 0.2f)
				{
					return ThoughtState.ActiveAtStage(1);
				}
			}
		}
		if (num < 0.5f)
		{
			return ThoughtState.ActiveAtStage(0);
		}
		return ThoughtState.Inactive;
	}
}
