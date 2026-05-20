using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_AncientUplink : TileMutatorWorker
{
	public TileMutatorWorker_AncientUplink(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostFog(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		List<CellRect> usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		PrefabDef prefab = PrefabDefOf.AncientUplink;
		if (!MapGenUtility.TryGetRandomClearRect(prefab.size.x, prefab.size.z, out var rect, -1, -1, Validator))
		{
			if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => Validator(CellRect.CenteredOn(c, prefab.size)), out var result))
			{
				return;
			}
			rect = CellRect.CenteredOn(result, prefab.size);
		}
		foreach (IntVec3 item in rect)
		{
			map.terrainGrid.SetTerrain(item, TerrainDefOf.AncientTile);
		}
		PrefabUtility.SpawnPrefab(prefab, map, GetPrefabRoot(rect), Rot4.North);
		usedRects.Add(rect);
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
			if (!PrefabUtility.CanSpawnPrefab(prefab, map, GetPrefabRoot(r), Rot4.North, canWipeEdifices: false))
			{
				return false;
			}
			return !usedRects.Any((CellRect ur) => ur.Overlaps(r));
		}
	}

	private IntVec3 GetPrefabRoot(CellRect rect)
	{
		IntVec3 centerCell = rect.CenterCell;
		if (rect.Width % 2 == 0)
		{
			centerCell.x--;
		}
		if (rect.Height % 2 == 0)
		{
			centerCell.z--;
		}
		return centerCell;
	}
}
