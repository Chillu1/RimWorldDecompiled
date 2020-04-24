using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_ShipChunkDrop : IncidentWorker
	{
		private static readonly Pair<int, float>[] CountChance = new Pair<int, float>[4]
		{
			new Pair<int, float>(1, 1f),
			new Pair<int, float>(2, 0.95f),
			new Pair<int, float>(3, 0.7f),
			new Pair<int, float>(4, 0.4f)
		};

		private int RandomCountToDrop
		{
			get
			{
				float x2 = (float)Find.TickManager.TicksGame / 3600000f;
				float timePassedFactor = Mathf.Clamp(GenMath.LerpDouble(0f, 1.2f, 1f, 0.1f, x2), 0.1f, 1f);
				return CountChance.RandomElementByWeight((Pair<int, float> x) => (x.First == 1) ? x.Second : (x.Second * timePassedFactor)).First;
			}
		}

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			IntVec3 pos;
			return TryFindShipChunkDropCell(map.Center, map, 999999, out pos);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindShipChunkDropCell(map.Center, map, 999999, out IntVec3 pos))
			{
				return false;
			}
			SpawnShipChunks(pos, map, RandomCountToDrop);
			Messages.Message("MessageShipChunkDrop".Translate(), new TargetInfo(pos, map), MessageTypeDefOf.NeutralEvent);
			return true;
		}

		private void SpawnShipChunks(IntVec3 firstChunkPos, Map map, int count)
		{
			SpawnChunk(firstChunkPos, map);
			for (int i = 0; i < count - 1; i++)
			{
				if (TryFindShipChunkDropCell(firstChunkPos, map, 5, out IntVec3 pos))
				{
					SpawnChunk(pos, map);
				}
			}
		}

		private void SpawnChunk(IntVec3 pos, Map map)
		{
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.ShipChunkIncoming, ThingDefOf.ShipChunk, pos, map);
		}

		private bool TryFindShipChunkDropCell(IntVec3 nearLoc, Map map, int maxDist, out IntVec3 pos)
		{
			return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.ShipChunkIncoming, map, out pos, 10, nearLoc, maxDist);
		}
	}
}
