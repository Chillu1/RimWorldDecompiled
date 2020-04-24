using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_ScatterLumpsMineable : GenStep_Scatterer
	{
		public ThingDef forcedDefToScatter;

		public int forcedLumpSize;

		public float maxValue = float.MaxValue;

		[Unsaved(false)]
		protected List<IntVec3> recentLumpCells = new List<IntVec3>();

		public override int SeedPart => 920906419;

		public override void Generate(Map map, GenStepParams parms)
		{
			minSpacing = 5f;
			warnOnFail = false;
			int num = CalculateFinalCount(map);
			for (int i = 0; i < num; i++)
			{
				if (!TryFindScatterCell(map, out IntVec3 result))
				{
					return;
				}
				ScatterAt(result, map, parms);
				usedSpots.Add(result);
			}
			usedSpots.Clear();
		}

		protected ThingDef ChooseThingDef()
		{
			if (forcedDefToScatter != null)
			{
				return forcedDefToScatter;
			}
			return DefDatabase<ThingDef>.AllDefs.RandomElementByWeightWithFallback(delegate(ThingDef d)
			{
				if (d.building == null)
				{
					return 0f;
				}
				return (d.building.mineableThing != null && d.building.mineableThing.BaseMarketValue > maxValue) ? 0f : d.building.mineableScatterCommonality;
			});
		}

		protected override bool CanScatterAt(IntVec3 c, Map map)
		{
			if (NearUsedSpot(c, minSpacing))
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

		protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
		{
			ThingDef thingDef = ChooseThingDef();
			if (thingDef != null)
			{
				int numCells = (forcedLumpSize > 0) ? forcedLumpSize : thingDef.building.mineableScatterLumpSizeRange.RandomInRange;
				recentLumpCells.Clear();
				foreach (IntVec3 item in GridShapeMaker.IrregularLump(c, map, numCells))
				{
					GenSpawn.Spawn(thingDef, item, map);
					recentLumpCells.Add(item);
				}
			}
		}
	}
}
