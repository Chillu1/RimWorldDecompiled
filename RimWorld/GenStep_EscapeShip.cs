using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_EscapeShip : GenStep_Scatterer
{
	private static readonly IntRange EscapeShipSizeWidth = new IntRange(20, 28);

	private static readonly IntRange EscapeShipSizeHeight = new IntRange(34, 42);

	public override int SeedPart => 860042045;

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (!base.CanScatterAt(c, map))
		{
			return false;
		}
		if (!c.Standable(map))
		{
			return false;
		}
		if (c.Roofed(map))
		{
			return false;
		}
		if (!map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors)))
		{
			return false;
		}
		CellRect cellRect = new CellRect(c.x - EscapeShipSizeWidth.min / 2, c.z - EscapeShipSizeHeight.min / 2, EscapeShipSizeWidth.min, EscapeShipSizeHeight.min);
		if (!cellRect.FullyContainedWithin(new CellRect(0, 0, map.Size.x, map.Size.z)))
		{
			return false;
		}
		foreach (IntVec3 item in cellRect)
		{
			TerrainDef terrainDef = map.terrainGrid.TerrainAt(item);
			if (!item.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy) && (terrainDef.driesTo == null || !terrainDef.driesTo.affordances.Contains(TerrainAffordanceDefOf.Heavy)))
			{
				return false;
			}
		}
		return true;
	}

	protected override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
	{
		int randomInRange = EscapeShipSizeWidth.RandomInRange;
		int randomInRange2 = EscapeShipSizeHeight.RandomInRange;
		CellRect rect = new CellRect(c.x - randomInRange / 2, c.z - randomInRange2 / 2, randomInRange, randomInRange2);
		rect.ClipInsideMap(map);
		foreach (IntVec3 item in rect)
		{
			if (item.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy))
			{
				continue;
			}
			CompTerrainPumpDry.AffectCell(map, item);
			for (int i = 0; i < 8; i++)
			{
				Vector3 vector = Rand.InsideUnitCircleVec3 * 3f;
				IntVec3 c2 = IntVec3.FromVector3(item.ToVector3Shifted() + vector);
				if (c2.InBounds(map))
				{
					CompTerrainPumpDry.AffectCell(map, c2);
				}
			}
		}
		ResolveParams resolveParams = new ResolveParams
		{
			rect = rect
		};
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.symbolStack.Push("ship_core", resolveParams);
		RimWorld.BaseGen.BaseGen.Generate();
	}
}
