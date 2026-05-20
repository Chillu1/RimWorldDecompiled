using Verse;

namespace RimWorld;

public class TileMutatorWorker_ArcheanTrees : TileMutatorWorker
{
	private float radius;

	private const int MinCellsAround = 5;

	private const float ExtraRange = 1.9f;

	private const float ExtraChance = 0.33f;

	private static readonly IntRange TreeRange = new IntRange(10, 15);

	public TileMutatorWorker_ArcheanTrees(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GenerateCriticalStructures(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		radius = ThingDefOf.Plant_TreeArchean.GetCompProperties<CompProperties_Terraformer>().radius;
		int randomInRange = TreeRange.RandomInRange;
		int num = 0;
		foreach (IntVec3 item in map.AllCells.InRandomOrder())
		{
			if (ValidPosition(item, map))
			{
				SpawnTree(item, map);
				if (++num >= randomInRange)
				{
					break;
				}
			}
		}
	}

	private void SpawnTree(IntVec3 cell, Map map)
	{
		WildPlantSpawner.SpawnPlant(ThingDefOf.Plant_TreeArchean, map, cell, setRandomGrowth: false).Growth = 1f;
		int num = GenRadial.NumCellsInRadius(radius + 1.9f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = cell + GenRadial.RadialPattern[i];
			float magnitude = (intVec - cell).Magnitude;
			if (intVec.InBounds(map) && CompTerraformer.CanEverConvertCell(intVec, map) && (magnitude <= radius || Rand.Chance(0.33f)))
			{
				map.terrainGrid.SetTerrain(intVec, TerrainDefOf.SoilRich);
			}
		}
		MapGenerator.UsedRects.Add(cell.RectAbout((int)radius / 2, (int)radius / 2));
	}

	private bool ValidPosition(IntVec3 center, Map map)
	{
		if (!CompTerraformer.CanEverConvertCell(center, map))
		{
			return false;
		}
		int num = GenRadial.NumCellsInRadius(radius);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (CompTerraformer.CanEverConvertCell(center + GenRadial.RadialPattern[i], map))
			{
				num2++;
			}
			if (num2 >= 5)
			{
				return true;
			}
		}
		return false;
	}
}
