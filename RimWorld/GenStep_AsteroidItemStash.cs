using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class GenStep_AsteroidItemStash : GenStep
{
	private const float ClearNoiseFreq = 0.03f;

	private const float ClearNoiseStrength = 5f;

	private static readonly IntRange StructureSizeRange = new IntRange(30, 40);

	private const int DoorClearArea = 100;

	public override int SeedPart => 234098237;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModLister.CheckOdyssey("Asteroid Item Stash"))
		{
			return;
		}
		List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		IntVec2 size = new IntVec2(StructureSizeRange.RandomInRange, StructureSizeRange.RandomInRange);
		if (!FindStashRect(map, size, out var rect))
		{
			return;
		}
		ModuleBase baseShape = new DistFromPointRects(new List<CellRect> { rect.ExpandedBy(3) });
		baseShape = MapNoiseUtility.AddDisplacementNoise(baseShape, 0.03f, 5f);
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (baseShape.GetValue(allCell) > 0f)
			{
				allCell.GetEdifice(map)?.Destroy();
			}
		}
		StructureGenParams parms2 = new StructureGenParams
		{
			size = size
		};
		LayoutWorker worker = LayoutDefOf.OrbitalItemStash.Worker;
		LayoutStructureSketch layoutStructureSketch = worker.GenerateStructureSketch(parms2);
		List<Thing> list = new List<Thing>();
		IntVec3 min = rect.Min;
		List<Thing> allSpawnedThings = list;
		worker.Spawn(layoutStructureSketch, map, min, null, allSpawnedThings);
		orGenerateVar.Add(rect);
		foreach (Thing item in list)
		{
			if (item.def.IsDoor)
			{
				ClearAreaAroundDoor(item.Position, map);
			}
		}
		map.OrbitalDebris = OrbitalDebrisDefOf.Asteroid;
	}

	private void ClearAreaAroundDoor(IntVec3 thingPosition, Map map)
	{
		foreach (IntVec3 item in GridShapeMaker.IrregularLump(thingPosition, map, 100))
		{
			Building edifice = item.GetEdifice(map);
			if (edifice != null && edifice.def.building.isNaturalRock)
			{
				edifice.Destroy();
			}
		}
	}

	private bool FindStashRect(Map map, IntVec2 size, out CellRect rect)
	{
		rect = CellRect.Empty;
		int num = 0;
		for (int i = 0; i < 100; i++)
		{
			if (RCellFinder.TryFindRandomCellNearWith(map.Center, (IntVec3 c) => c.GetEdifice(map) != null, map, out var result))
			{
				CellRect cellRect = CellRect.CenteredOn(result, size);
				int num2 = cellRect.Cells.Count((IntVec3 c) => c.GetEdifice(map) != null);
				if (num2 > num)
				{
					num = num2;
					rect = cellRect;
				}
			}
		}
		return (float)num > (float)rect.Area / 2f;
	}
}
