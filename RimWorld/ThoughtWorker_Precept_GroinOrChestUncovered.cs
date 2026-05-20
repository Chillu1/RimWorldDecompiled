using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_Precept_GroinOrChestUncovered : ThoughtWorker_Precept
	{
		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			return HasUncoveredGroinOrChest(p);
		}

		public static bool HasUncoveredGroinOrChest(Pawn p)
		{
			if (p.apparel == null)
			{
				return false;
			}
			bool flag = false;
			bool flag2 = false;
			List<Apparel> wornApparel = p.apparel.WornApparel;
			for (int i = 0; i < wornApparel.Count; i++)
			{
				Apparel apparel = wornApparel[i];
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
				if (flag && flag2)
				{
					break;
				}
			}
			if (flag)
			{
				return !flag2;
			}
			return true;
		}
	}
}
