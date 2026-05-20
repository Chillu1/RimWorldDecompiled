using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_MasochistWearingBodyStrap : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (ModsConfig.IdeologyActive)
			{
				List<Apparel> wornApparel = p.apparel.WornApparel;
				for (int i = 0; i < wornApparel.Count; i++)
				{
					if (wornApparel[i].def == ThingDefOf.Apparel_BodyStrap)
					{
						return true;
					}
				}
			}
			return ThoughtState.Inactive;
		}
	}
}
