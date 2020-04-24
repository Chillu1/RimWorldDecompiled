using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterShrines : GenStep_ScatterRuinsSimple
	{
		private static readonly IntRange SizeRange = new IntRange(15, 20);

		public override int SeedPart => 1801222485;

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (!base.CanScatterAt(c, map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			if (edifice == null || !edifice.def.building.isNaturalRock)
			{
				return false;
			}
			return true;
		}

		protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1)
		{
			int randomInRange = SizeRange.RandomInRange;
			int randomInRange2 = SizeRange.RandomInRange;
			CellRect rect = new CellRect(loc.x, loc.z, randomInRange, randomInRange2);
			rect.ClipInsideMap(map);
			if (rect.Width == randomInRange && rect.Height == randomInRange2)
			{
				foreach (IntVec3 cell in rect.Cells)
				{
					List<Thing> list = map.thingGrid.ThingsListAt(cell);
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].def == ThingDefOf.AncientCryptosleepCasket)
						{
							return;
						}
					}
				}
				if (CanPlaceAncientBuildingInRange(rect, map))
				{
					ResolveParams resolveParams = default(ResolveParams);
					resolveParams.rect = rect;
					resolveParams.disableSinglePawn = true;
					resolveParams.disableHives = true;
					resolveParams.makeWarningLetter = true;
					RimWorld.BaseGen.BaseGen.globalSettings.map = map;
					RimWorld.BaseGen.BaseGen.symbolStack.Push("ancientTemple", resolveParams);
					RimWorld.BaseGen.BaseGen.Generate();
				}
			}
		}
	}
}
