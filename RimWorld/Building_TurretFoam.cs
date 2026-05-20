using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Building_TurretFoam : Building_TurretGun
{
	public override bool IsEverThreat => false;

	public override LocalTargetInfo TryFindNewTarget()
	{
		int num = GenRadial.NumCellsInRadius(AttackVerb.EffectiveRange);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = base.Position + GenRadial.RadialPattern[i];
			if (!GenSight.LineOfSight(base.Position, intVec, base.Map, skipFirstCell: true))
			{
				continue;
			}
			List<Thing> thingList = intVec.GetThingList(base.Map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (thingList[j] is Fire || thingList[j].HasAttachment(ThingDefOf.Fire))
				{
					return thingList[j].Position;
				}
			}
		}
		return LocalTargetInfo.Invalid;
	}
}
