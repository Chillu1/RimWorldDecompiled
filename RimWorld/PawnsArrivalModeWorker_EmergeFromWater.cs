using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class PawnsArrivalModeWorker_EmergeFromWater : PawnsArrivalModeWorker
{
	private const int LargeBodyOfWaterThreshold = 50;

	private const float MaxSpawnRadius = 30f;

	private const float SpawnDurationSeconds = 8f;

	private const int SpawnDelayTicks = 40;

	public static readonly FloatRange StunRange = new FloatRange(2f, 4f);

	private static readonly HashSet<IntVec3> WaterBodyCells = new HashSet<IntVec3>();

	public override bool CanUseWith(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (map.TileInfo.Isnt<SurfaceTile>(out var casted))
		{
			return false;
		}
		bool num = !casted.Rivers.NullOrEmpty();
		bool isCoastal = map.TileInfo.IsCoastal;
		return num || isCoastal;
	}

	public override void Arrive(List<Pawn> pawns, IncidentParms parms)
	{
		if (!pawns.Empty())
		{
			List<IntVec3> list = new List<IntVec3>();
			List<IntVec3> list2 = new List<IntVec3>();
			for (int i = 0; i < pawns.Count; i++)
			{
				IntVec3 item = WaterBodyCells.RandomElement();
				WaterBodyCells.Remove(item);
				(IsBigSpawn(pawns[i]) ? list2 : list).Add(item);
				pawns[i].stances.stunner.StunFor(StunRange.RandomInRangeSeeded(pawns[i].ThingID.GetHashCode()).SecondsToTicks(), pawns[i], addBattleLog: true, showMote: false);
			}
			float num = 8f / (float)pawns.Count;
			SpawnRequest spawnRequest = new SpawnRequest(pawns.Where((Pawn p) => !IsBigSpawn(p)).Cast<Thing>().ToList(), list, 1, num);
			SpawnRequest spawnRequest2 = new SpawnRequest(pawns.Where((Pawn p) => IsBigSpawn(p)).Cast<Thing>().ToList(), list2, 1, num * 2.7f);
			spawnRequest.spawnSound = SoundDefOf.EmergeFromWater;
			spawnRequest.spawnEffect = EffecterDefOf.PawnEmergeFromWater;
			spawnRequest.preSpawnEffect = EffecterDefOf.WaterMist;
			spawnRequest.preSpawnEffecterOffsetTicks = -40;
			spawnRequest.initialDelay = 40;
			spawnRequest.lord = parms.lord;
			spawnRequest2.spawnSound = SoundDefOf.EmergeFromWater;
			spawnRequest2.spawnEffect = EffecterDefOf.PawnEmergeFromWaterLarge;
			spawnRequest2.preSpawnEffect = EffecterDefOf.WaterMist;
			spawnRequest2.preSpawnEffecterOffsetTicks = -40;
			spawnRequest2.initialDelay = 40;
			spawnRequest2.lord = parms.lord;
			Find.CurrentMap.deferredSpawner.AddRequest(spawnRequest);
			Find.CurrentMap.deferredSpawner.AddRequest(spawnRequest2);
		}
	}

	private bool IsBigSpawn(Pawn pawn)
	{
		return pawn.BodySize > 1.5f;
	}

	public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		int num = 20;
		while (true)
		{
			_ = parms.spawnCenter;
			if (!(parms.spawnCenter == IntVec3.Invalid) || num <= 0)
			{
				break;
			}
			WaterBodyCells.Clear();
			parms.spawnCenter = CellFinderLoose.RandomCellWith((IntVec3 cell) => (cell.GetTerrain(map).IsRiver || cell.GetTerrain(map).IsOcean) && cell.Walkable(map) && map.reachability.CanReachColony(cell), map);
			if (parms.spawnCenter != IntVec3.Invalid)
			{
				map.floodFiller.FloodFill(parms.spawnCenter, (IntVec3 cell) => cell.GetTerrain(map).IsWater && cell.Walkable(map) && cell.InHorDistOf(parms.spawnCenter, 30f), delegate(IntVec3 cell)
				{
					WaterBodyCells.Add(cell);
				});
				if (WaterBodyCells.Count > 50)
				{
					return true;
				}
			}
			num--;
		}
		return false;
	}
}
