using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_FaceCovered : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return HasCoveredFace(p);
		}

		public static bool HasCoveredFace(Pawn p)
		{
			if (p.apparel == null)
			{
				return false;
			}
			List<Apparel> wornApparel = p.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Apparel apparel = wornApparel[i];
				for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
				{
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.UpperHead)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
