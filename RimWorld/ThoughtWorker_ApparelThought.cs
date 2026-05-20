using System.Collections.Generic;
using Verse;

namespace RimWorld;

public abstract class ThoughtWorker_ApparelThought : ThoughtWorker
{
	protected abstract bool ApparelCounts(Apparel apparel);

	protected override ThoughtState CurrentStateInternal(Pawn p)
	{
		string text = null;
		int num = 0;
		List<Apparel> wornApparel = p.apparel.WornApparel;
		for (int i = 0; i < wornApparel.Count; i++)
		{
			if (ApparelCounts(wornApparel[i]))
			{
				if (text == null)
				{
					text = wornApparel[i].def.label;
				}
				num++;
			}
		}
		if (num == 0)
		{
			return ThoughtState.Inactive;
		}
		if (num >= 5)
		{
			return ThoughtState.ActiveAtStage(4, text);
		}
		return ThoughtState.ActiveAtStage(num - 1, text);
	}
}
