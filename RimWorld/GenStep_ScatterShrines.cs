using System.Collections.Generic;
using RimWorld.BaseGen;
using Verse;

namespace RimWorld;

public class GenStep_ScatterShrines : GenStep_ScatterRuinsSimple
{
	private IntVec2 randomSize;

	private static readonly IntRange SizeRange = new IntRange(15, 20);

	private const float SkipNonStartingMapChance = 0.75f;

	public override int SeedPart => 1801222485;

	protected override bool TryFindScatterCell(Map map, out IntVec3 result)
	{
		randomSize.x = SizeRange.RandomInRange;
		randomSize.z = SizeRange.RandomInRange;
		return base.TryFindScatterCell(map, out result);
	}

	protected override bool ShouldSkipMap(Map map)
	{
		if (!map.IsStartingMap && Rand.Chance(0.75f))
		{
			return true;
		}
		return false;
	}

	protected override bool CanScatterAt(IntVec3 c, Map map)
	{
		if (!base.CanScatterAt(c, map))
		{
			return false;
		}
		for (int i = 0; i < 9; i++)
		{
			IntVec3 c2 = c + GenAdj.AdjacentCellsAndInside[i];
			if (c2.InBounds(map))
			{
				Building edifice = c2.GetEdifice(map);
				if (edifice != null && edifice.def.building.isNaturalRock)
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override CellRect EffectiveRectAt(IntVec3 c)
	{
		return CellRect.CenteredOn(c, randomSize.x, randomSize.z);
	}

	protected override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int stackCount = 1)
	{
		List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		CellRect cellRect = EffectiveRectAt(loc);
		CellRect cellRect2 = cellRect.ClipInsideMap(map);
		if (cellRect2.Width != cellRect.Width || cellRect2.Height != cellRect.Height)
		{
			return;
		}
		foreach (IntVec3 cell in cellRect.Cells)
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
		if (CanPlaceAncientBuildingInRange(cellRect, map) && !orGenerateVar.Contains(cellRect))
		{
			orGenerateVar.Add(cellRect);
			ResolveParams resolveParams = new ResolveParams
			{
				rect = cellRect,
				disableSinglePawn = true,
				disableHives = true,
				makeWarningLetter = true
			};
			if (Find.Storyteller.difficulty.peacefulTemples)
			{
				resolveParams.podContentsType = PodContentsType.AncientFriendly;
			}
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("ancientTemple", resolveParams);
			RimWorld.BaseGen.BaseGen.Generate();
		}
	}
}
