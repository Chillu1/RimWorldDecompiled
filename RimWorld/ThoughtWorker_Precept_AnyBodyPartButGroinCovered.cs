using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_AnyBodyPartButGroinCovered : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return HasCoveredBodyPartsButGroin(p) && GenTemperature.SafeTemperatureRange(p.def).Includes(p.AmbientTemperature);
		}

		public static bool HasCoveredBodyPartsButGroin(Pawn p)
		{
			if (p.apparel == null)
			{
				return false;
			}
			List<Apparel> wornApparel = p.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Apparel apparel = wornApparel[i];
				if ((p.kindDef.apparelRequired != null && p.kindDef.apparelRequired.Contains(apparel.def)) || !apparel.def.apparel.countsAsClothingForNudity)
				{
					continue;
				}
				bool flag = false;
				bool flag2 = false;
				for (int j = 0; j < apparel.def.apparel.bodyPartGroups.Count; j++)
				{
					if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Torso)
					{
						flag = true;
					}
					else if (apparel.def.apparel.bodyPartGroups[j] == BodyPartGroupDefOf.Legs)
					{
						flag2 = true;
					}
				}
				if (flag || !flag2)
				{
					return true;
				}
			}
			return false;
		}
	}
}
