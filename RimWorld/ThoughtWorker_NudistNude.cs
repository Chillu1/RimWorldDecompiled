using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NudistNude : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			List<Apparel> wornApparel = p.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Apparel apparel = wornApparel[i];
				for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
				{
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Torso)
					{
						return false;
					}
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Legs)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
