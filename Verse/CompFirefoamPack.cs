using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class CompFirefoamPack : CompAIUsablePack
{
	private const float ChanceToUseWithNearbyFire = 0.2f;

	protected override float ChanceToUse(Pawn wearer)
	{
		if (wearer.GetAttachment(ThingDefOf.Fire) != null)
		{
			return 1f;
		}
		int num = GenRadial.NumCellsInRadius(1.9f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = wearer.Position + GenRadial.RadialPattern[i];
			if (!c.InBounds(wearer.Map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(wearer.Map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (thingList[j] is Fire || thingList[j].HasAttachment(ThingDefOf.Fire))
				{
					if (i == 0)
					{
						return 1f;
					}
					return 0.2f;
				}
			}
		}
		return 0f;
	}

	protected override void UsePack(Pawn wearer)
	{
		Verb_FirefoamPop.Pop(wearer, parent.TryGetComp<CompExplosive>(), parent.TryGetComp<CompApparelReloadable>());
	}
}
