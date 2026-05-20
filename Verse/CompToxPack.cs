using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class CompToxPack : CompAIUsablePack
{
	protected override float ChanceToUse(Pawn wearer)
	{
		if (!ModsConfig.BiotechActive)
		{
			return 0f;
		}
		int num = GenRadial.NumCellsInRadius(1.9f);
		float num2 = 0f;
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
				if (thingList[j] is Pawn pawn && pawn != wearer && pawn.HostileTo(wearer) && GasUtility.IsAffectedByExposure(pawn) && !pawn.IsPsychologicallyInvisible())
				{
					num2 += pawn.BodySize;
					if (num2 >= 1f)
					{
						break;
					}
				}
			}
		}
		return num2;
	}

	protected override void UsePack(Pawn wearer)
	{
		Verb_DeployToxPack.TryDeploy(parent.TryGetComp<CompApparelReloadable>(), parent.TryGetComp<CompReleaseGas>());
	}
}
