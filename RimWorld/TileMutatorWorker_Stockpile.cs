using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld.SketchGen;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_Stockpile : TileMutatorWorker
{
	public enum StockpileType
	{
		Medicine,
		Chemfuel,
		Component,
		Weapons,
		Gravcore,
		Drugs
	}

	public static readonly List<StockpileType> GeneratableStockpileTypes = new List<StockpileType>
	{
		StockpileType.Medicine,
		StockpileType.Chemfuel,
		StockpileType.Component,
		StockpileType.Weapons,
		StockpileType.Drugs
	};

	public TileMutatorWorker_Stockpile(TileMutatorDef def)
		: base(def)
	{
	}

	public override string GetLabel(PlanetTile tile)
	{
		return ("Stockpile" + GetStockpileType(tile)).Translate();
	}

	public override void GeneratePostFog(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		Sketch sketch = RimWorld.SketchGen.SketchGen.Generate(parms: new SketchResolveParams
		{
			sketch = new Sketch()
		}, root: SketchResolverDefOf.AncientHatch);
		if (!MapGenUtility.TryGetRandomClearRect(sketch.OccupiedSize.x, sketch.OccupiedSize.z, out var rect, -1, -1, Validator))
		{
			if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(CellRect.CenteredOn(c, sketch.OccupiedSize)), out var result))
			{
				return;
			}
			rect = CellRect.CenteredOn(result, sketch.OccupiedSize);
		}
		List<Thing> list = new List<Thing>();
		sketch.Spawn(map, rect.Min, null, Sketch.SpawnPosType.Unchanged, Sketch.SpawnMode.Normal, wipeIfCollides: true, forceTerrainAffordance: false, clearEdificeWhereFloor: true, list);
		usedRects.Add(rect);
		(list.FirstOrDefault((Thing t) => t.def == ThingDefOf.AncientHatch) as AncientHatch).stockpileType = GetStockpileType(map.Tile);
		bool Validator(CellRect r)
		{
			if (!r.InBounds(map))
			{
				return false;
			}
			if (r.Cells.Any((IntVec3 c) => c.Fogged(map)))
			{
				return false;
			}
			if (!usedRects.Any((CellRect ur) => ur.Overlaps(r)))
			{
				return !sketch.IsSpawningBlocked(map, r.Min, null);
			}
			return false;
		}
	}

	private static StockpileType GetStockpileType(PlanetTile tile)
	{
		Rand.PushState(tile.tileId);
		StockpileType result = GeneratableStockpileTypes.RandomElement();
		Rand.PopState();
		return result;
	}
}
