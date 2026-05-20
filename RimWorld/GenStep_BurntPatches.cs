using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class GenStep_BurntPatches : GenStep
{
	private static readonly IntRange PatchCountRange = new IntRange(0, 2);

	private static readonly IntRange PatchSizeRange = new IntRange(50, 200);

	public override int SeedPart => 823478234;

	public override void Generate(Map map, GenStepParams parms)
	{
		int randomInRange = PatchCountRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			GeneratePatch(map);
		}
	}

	private void GeneratePatch(Map map)
	{
		if (!RCellFinder.TryFindRandomCellNearWith(map.Center, (IntVec3 c) => c.Standable(map), map, out var result, 30))
		{
			return;
		}
		foreach (IntVec3 item in GridShapeMaker.IrregularLump(result, map, PatchSizeRange.RandomInRange))
		{
			List<Thing> thingList = item.GetThingList(map);
			for (int num = thingList.Count - 1; num >= 0; num--)
			{
				Thing thing = thingList[num];
				if (thing.def.IsPlant)
				{
					thing.Kill(new DamageInfo(DamageDefOf.Flame, 9999f));
				}
			}
			map.snowGrid.SetDepth(item, 0f);
			FilthMaker.TryMakeFilth(item, map, ThingDefOf.Filth_Ash);
		}
	}
}
