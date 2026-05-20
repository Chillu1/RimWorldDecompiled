using System.Collections.Generic;
using System.Linq;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_MechanoidRelay : GenStep
{
	public override int SeedPart => 583937484;

	public override void Generate(Map map, GenStepParams parms)
	{
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		foreach (Thing thing in (IEnumerable<Thing>)parms.sitePart.things)
		{
			ResolveParams resolveParams = new ResolveParams
			{
				singleThingToSpawn = thing
			};
			if (thing.def != ThingDefOf.MechRelay || !RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => CellValid(x, map, thing.def.Size, usedRects), map, out var result))
			{
				result = CellFinderLoose.RandomCellWith((IntVec3 x) => CellValid(x, map, thing.def.Size, usedRects), map);
			}
			if (!result.IsValid)
			{
				Log.Warning("Could not find valid cell for MechRelay/Stabilizer.");
			}
			resolveParams.rect = GenAdj.OccupiedRect(result, thing.def.defaultPlacingRot, thing.def.size);
			ChangeTerrainAround(resolveParams, map, thing, usedRects);
			RimWorld.BaseGen.BaseGen.symbolStack.Push("thing", resolveParams);
		}
		RimWorld.BaseGen.BaseGen.globalSettings.map = map;
		RimWorld.BaseGen.BaseGen.Generate();
	}

	private void ChangeTerrainAround(ResolveParams thingParms, Map map, Thing thing, List<CellRect> usedRects)
	{
		CellRect cellRect = thingParms.rect.ExpandedBy(1);
		foreach (IntVec3 item2 in cellRect)
		{
			if (map.terrainGrid.TerrainAt(item2).changeable)
			{
				map.terrainGrid.SetTerrain(item2, TerrainDefOf.AncientConcrete);
			}
		}
		CellRect item = cellRect.ExpandedBy(1);
		bool flag = thing.def == ThingDefOf.MechRelay;
		IEnumerable<IntVec3> corners = item.Corners;
		foreach (IntVec3 edgeCell in item.EdgeCells)
		{
			if ((flag || !corners.Contains(edgeCell)) && map.terrainGrid.TerrainAt(edgeCell).changeable)
			{
				map.terrainGrid.SetTerrain(edgeCell, TerrainDefOf.AncientTile);
			}
		}
		usedRects.Add(item);
		if (!flag)
		{
			return;
		}
		foreach (IntVec3 item3 in corners)
		{
			if (GenConstruct.TerrainCanSupport(CellRect.CenteredOn(item3, 1), map, ThingDefOf.AncientMechDropBeacon))
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.AncientMechDropBeacon), item3, map, ThingDefOf.AncientMechDropBeacon.defaultPlacingRot);
			}
		}
	}

	private bool CellValid(IntVec3 cell, Map map, IntVec2 size, List<CellRect> usedRects)
	{
		CellRect other = CellRect.CenteredOn(cell, Mathf.Max(size.x, size.z)).ExpandedBy(2);
		foreach (CellRect usedRect in usedRects)
		{
			if (usedRect.Overlaps(other))
			{
				return false;
			}
		}
		foreach (IntVec3 item in other)
		{
			if (!item.InBounds(map))
			{
				return false;
			}
			if (item.Fogged(map))
			{
				return false;
			}
			if (!item.Walkable(map))
			{
				return false;
			}
		}
		if (!cell.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy))
		{
			return false;
		}
		return true;
	}
}
