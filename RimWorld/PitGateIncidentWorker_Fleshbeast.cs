using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class PitGateIncidentWorker_Fleshbeast : PitGateIncidentWorker
{
	private const int FleshBeastEmergenceDuration = 600;

	private Lord lord;

	public override void Setup(float points)
	{
		base.Setup(points);
		List<Pawn> fleshbeastsForPoints = FleshbeastUtility.GetFleshbeastsForPoints(points, pitGate.Map);
		List<PawnFlyer> list = new List<PawnFlyer>();
		List<IntVec3> list2 = new List<IntVec3>();
		CellRect cellRect = GenAdj.OccupiedRect(pitGate.Position, Rot4.North, ThingDefOf.PitGate.Size).ContractedBy(2);
		foreach (Pawn item in fleshbeastsForPoints)
		{
			IntVec3 randomCell = cellRect.RandomCell;
			GenSpawn.Spawn(item, randomCell, pitGate.Map);
			CellFinder.TryFindRandomCellNear(pitGate.Position, pitGate.Map, ThingDefOf.PitGate.size.x / 2 + 1, (IntVec3 cell) => !cell.Fogged(pitGate.Map) && cell.Walkable(pitGate.Map) && !cell.Impassable(pitGate.Map), out var result);
			item.rotationTracker.FaceCell(result);
			list.Add(PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer_Stun, item, result, null, null, flyWithCarriedThing: false, randomCell.ToVector3() + new Vector3(0f, 0f, -1f)));
			list2.Add(randomCell);
		}
		float intervalSeconds = 600.TicksToSeconds() / (float)fleshbeastsForPoints.Count;
		SpawnRequest spawnRequest = new SpawnRequest(list.Cast<Thing>().ToList(), list2, 1, intervalSeconds);
		spawnRequest.lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_FleshbeastAssault(), pitGate.Map);
		pitGate.Map.deferredSpawner.AddRequest(spawnRequest);
		SoundDefOf.Pawn_Fleshbeast_EmergeFromPitGate.PlayOneShot(pitGate);
	}
}
