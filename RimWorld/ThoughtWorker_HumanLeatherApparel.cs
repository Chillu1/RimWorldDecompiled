using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_HumanLeatherApparel : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			string text = null;
			int num = 0;
			List<Apparel> wornApparel = p.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				if (wornApparel[i].Stuff == ThingDefOf.Human.race.leatherDef)
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
}
