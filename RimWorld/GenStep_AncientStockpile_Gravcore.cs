using System.Collections.Generic;
using RimWorld.SketchGen;
using Verse;

namespace RimWorld;

public class GenStep_AncientStockpile_Gravcore : GenStep
{
	private List<Thing> hatches = new List<Thing>();

	private const int NumHatches = 3;

	private const int MinDistanceBetweenHatches = 20;

	private static List<Thing> spawnedThings = new List<Thing>();

	public override int SeedPart => 341298741;

	public override void Generate(Map map, GenStepParams parms)
	{
		for (int i = 0; i < 3; i++)
		{
			GenerateHatch(map, i == 0);
		}
	}

	private void GenerateHatch(Map map, bool spawnGravcore = false)
	{
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		Sketch sketch = RimWorld.SketchGen.SketchGen.Generate(parms: new SketchResolveParams
		{
			sketch = new Sketch()
		}, root: SketchResolverDefOf.AncientHatch);
		if (!MapGenUtility.TryGetRandomClearRect(sketch.OccupiedSize.x, sketch.OccupiedSize.z, out var rect, -1, -1, Validator))
		{
			if (!CellFinder.TryFindRandomCell(map, (IntVec3 cell) => Validator(CellRect.CenteredOn(cell, sketch.OccupiedSize)), out var result))
			{
				return;
			}
			rect = CellRect.CenteredOn(result, sketch.OccupiedSize);
		}
		spawnedThings.Clear();
		sketch.Spawn(map, rect.Min, null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: true, forceTerrainAffordance: false, clearEdificeWhereFloor: true, spawnedThings);
		usedRects.Add(rect);
		AncientHatch ancientHatch = spawnedThings.FirstOrDefault((Thing t) => t.def == ThingDefOf.AncientHatch) as AncientHatch;
		if (spawnGravcore)
		{
			ancientHatch.stockpileType = TileMutatorWorker_Stockpile.StockpileType.Gravcore;
		}
		else
		{
			ancientHatch.layout = LayoutDefOf.AncientStockpileFake;
		}
		hatches.Add(ancientHatch);
		bool Validator(CellRect r)
		{
			if (hatches.Any((Thing h) => r.CenterCell.InHorDistOf(h.Position, 20f)))
			{
				return false;
			}
			if (usedRects.Any((CellRect ur) => ur.Overlaps(r)))
			{
				return false;
			}
			if (sketch.IsSpawningBlocked(map, r.Min, null))
			{
				return false;
			}
			if (r.CenterCell.Fogged(map))
			{
				return false;
			}
			return true;
		}
	}
}
