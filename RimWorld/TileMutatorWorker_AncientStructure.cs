using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_AncientStructure : TileMutatorWorker
{
	private CellRect structurePerimeterRect;

	public TileMutatorWorker_AncientStructure(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GenerateCriticalStructures(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
			IntRange structureSizeRange = new IntRange(def.structureGenParms.structureSizeRange.min + def.structureGenParms.perimeterExpandBy * 2, def.structureGenParms.structureSizeRange.max + def.structureGenParms.perimeterExpandBy * 2);
			if (!MapGenUtility.TryGetStructureRect(map, structureSizeRange, out structurePerimeterRect))
			{
				structurePerimeterRect = map.Center.RectAbout(structureSizeRange.RandomInRange, structureSizeRange.RandomInRange);
			}
			orGenerateVar.Add(structurePerimeterRect);
			CellRect var = structurePerimeterRect.ContractedBy(def.structureGenParms.perimeterExpandBy);
			MapGenerator.SetVar("SpawnRect", var);
			StructureGenParams parms = new StructureGenParams
			{
				size = var.Size
			};
			LayoutWorker worker = def.structureGenParms.structureLayoutDef.Worker;
			LayoutStructureSketch layoutStructureSketch = worker.GenerateStructureSketch(parms);
			map.layoutStructureSketches.Add(layoutStructureSketch);
			worker.Spawn(layoutStructureSketch, map, var.Min);
			GeneratePerimeter(layoutStructureSketch, map);
		}
	}

	public override void GenerateNonCriticalStructures(Map map)
	{
		MapGenUtility.SpawnScatteredGroupPrefabs(map, structurePerimeterRect, def.structureGenParms.scatteredPrefabs);
	}

	private void GeneratePerimeter(LayoutStructureSketch layoutStructureSketch, Map map)
	{
		if (def.structureGenParms.perimeterWallDef == null && def.structureGenParms.perimeterTerrainDef == null)
		{
			return;
		}
		CellRect cellRect = layoutStructureSketch.structureLayout.container.ExpandedBy(def.structureGenParms.perimeterExpandBy);
		if (def.structureGenParms.perimeterTerrainDef != null)
		{
			foreach (IntVec3 cell2 in cellRect.Cells)
			{
				if (PerimeterValidator(cell2, def.structureGenParms.perimeterTerrainDef))
				{
					map.terrainGrid.SetTerrain(cell2, def.structureGenParms.perimeterTerrainDef);
				}
			}
		}
		float num = (float)cellRect.Area / 100f;
		foreach (LayoutScatterTerrainParms parms in def.structureGenParms.perimeterScatterTerrain)
		{
			int a = ((!(parms.groupCount != IntRange.Invalid)) ? Mathf.RoundToInt(parms.groupsPerHundredCells.RandomInRange * num) : parms.groupCount.RandomInRange);
			a = Mathf.Max(a, parms.minGroups);
			for (int i = 0; i < a; i++)
			{
				int randomInRange = parms.groupDistRange.RandomInRange;
				int randomInRange2 = parms.itemsPerGroup.RandomInRange;
				int num2 = 0;
				cellRect.TryFindRandomCell(out var cell, (IntVec3 c2) => PerimeterValidator(c2, parms.def));
				List<IntVec3> list = GridShapeMaker.IrregularLump(cell, map, randomInRange, (IntVec3 c2) => PerimeterValidator(c2, parms.def));
				for (int num3 = 0; num3 < list.Count; num3++)
				{
					IntVec3 c = list[num3];
					if (PerimeterValidator(c, parms.def) && Rand.DynamicChance(num2, randomInRange2, list.Count - num3))
					{
						num2++;
						map.terrainGrid.SetTerrain(c, parms.def);
					}
				}
			}
		}
		if (def.structureGenParms.perimeterWallDef != null)
		{
			foreach (IntVec3 edgeCell in cellRect.EdgeCells)
			{
				if (Rand.Chance(def.structureGenParms.perimeterWallChance) && !layoutStructureSketch.AnyRoomContains(edgeCell) && GenSpawn.CanSpawnAt(def.structureGenParms.perimeterWallDef, edgeCell, map, null, canWipeEdifices: false))
				{
					GenSpawn.Spawn(def.structureGenParms.perimeterWallDef, edgeCell, map);
				}
			}
		}
		if (def.structureGenParms.perimeterScatteredThings.NullOrEmpty())
		{
			return;
		}
		foreach (IntVec3 cell3 in cellRect.Cells)
		{
			if (layoutStructureSketch.AnyRoomContains(cell3))
			{
				continue;
			}
			AncientStructureGenParms.ScatteredThing scatteredThing = def.structureGenParms.perimeterScatteredThings.RandomElement();
			if (Rand.Chance(scatteredThing.chancePer100Cells / 100f))
			{
				Rot4 rot = (scatteredThing.thingDef.rotatable ? Rot4.Random : scatteredThing.thingDef.defaultPlacingRot);
				if (GenSpawn.CanSpawnAt(scatteredThing.thingDef, cell3, map, rot, canWipeEdifices: false))
				{
					GenSpawn.Spawn(scatteredThing.thingDef, cell3, map, rot);
				}
			}
		}
		bool PerimeterValidator(IntVec3 c2, TerrainDef def)
		{
			if (!c2.InBounds(map))
			{
				return false;
			}
			if (c2.GetEdifice(map) != null)
			{
				return false;
			}
			if (layoutStructureSketch.AnyRoomContains(c2))
			{
				return false;
			}
			if (!GenConstruct.CanBuildOnTerrain(def, c2, map, Rot4.North))
			{
				return false;
			}
			return true;
		}
	}
}
