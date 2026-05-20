using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public abstract class TileMutatorWorker_AncientVent : TileMutatorWorker
{
	private const float RadiusToClear = 8.9f;

	private const float ChanceNoMegaStructure = 0f;

	private static readonly IntRange numVentsRange = new IntRange(5, 9);

	private int numToGenerate;

	private List<IntVec3> positionsToGenerate = new List<IntVec3>();

	protected abstract ThingDef AncientVentDef { get; }

	public TileMutatorWorker_AncientVent(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GenerateNonCriticalStructures(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		numToGenerate = numVentsRange.RandomInRange;
		positionsToGenerate.Clear();
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		while (positionsToGenerate.Count < numToGenerate)
		{
			if (!CellFinder.TryFindRandomCell(map, Validator, out var result))
			{
				Log.Warning(string.Format("{0} failed to find any valid cells to place at least one vent. Placed {1} vents, wanted to place {2}.", "TileMutatorWorker_AncientVent", positionsToGenerate.Count, numToGenerate));
				break;
			}
			positionsToGenerate.Add(result);
		}
		TerrainGrid terrainGrid = map.terrainGrid;
		positionsToGenerate.RemoveWhere((IntVec3 p) => GenRadial.RadialCellsAround(p, 0f, 8.9f).Any((IntVec3 c) => map.terrainGrid.TerrainAt(c).IsWater));
		foreach (IntVec3 item in positionsToGenerate)
		{
			foreach (IntVec3 item2 in GenRadial.RadialCellsAround(item, 0f, 8.9f))
			{
				if (item2.InBounds(map) && item2.GetEdifice(map) == null && !Rand.Chance(0f))
				{
					terrainGrid.SetTerrain(item2, TerrainDefOf.AncientMegastructure);
				}
			}
		}
		foreach (IntVec3 item3 in positionsToGenerate)
		{
			Thing thing = ThingMaker.MakeThing(AncientVentDef);
			GenSpawn.Spawn(thing, item3, map);
			usedRects.Add(thing.OccupiedRect());
		}
		bool Validator(IntVec3 c)
		{
			if (c.DistanceToEdge(map) <= map.Size.x / 8)
			{
				return false;
			}
			if (!GenSpawn.CanSpawnAt(AncientVentDef, c, map, null, canWipeEdifices: false))
			{
				return false;
			}
			if (usedRects.Any((CellRect r) => r.Overlaps(GenAdj.OccupiedRect(c, Rot4.North, AncientVentDef.size))))
			{
				return false;
			}
			if (positionsToGenerate.Any((IntVec3 p) => p.InHorDistOf(c, 25f)))
			{
				return false;
			}
			return true;
		}
	}
}
