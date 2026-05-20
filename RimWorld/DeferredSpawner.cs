using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld;

public class DeferredSpawner : IExposable
{
	public Map map;

	private HashSet<SpawnRequest> requests = new HashSet<SpawnRequest>();

	public DeferredSpawner(Map map)
	{
		this.map = map;
	}

	public void DeferredSpawnerTick()
	{
		requests.RemoveWhere((SpawnRequest r) => r.done);
		foreach (SpawnRequest request in requests)
		{
			TickRequest(request);
		}
	}

	public void AddRequest(SpawnRequest request, bool spawnFirstBatchImmediately = true)
	{
		if (request.spawnPositions.Count != request.unspawnedThings.Count)
		{
			Log.Error("Spawn positions count does not match things to spawn count!");
			return;
		}
		requests.Add(request);
		if (spawnFirstBatchImmediately)
		{
			SpawnBatch(request, request.unspawnedThings.Count);
		}
	}

	public SpawnRequest GetRequestByLord(Lord lord)
	{
		return requests.FirstOrDefault((SpawnRequest r) => r.lord == lord);
	}

	private void TickRequest(SpawnRequest request)
	{
		int count = request.unspawnedThings.Count;
		if (request.preSpawnEffect != null && Find.TickManager.TicksGame >= request.PreEffecterTick && map.IsHashIntervalTick(request.intervalTicks, request.preSpawnEffecterOffsetTicks) && count > 0)
		{
			SpawnPreSpawnEffecter(request, count);
		}
		if (Find.TickManager.TicksGame >= request.startedTick + request.initialDelay && map.IsHashIntervalTick(request.intervalTicks) && count > 0)
		{
			SpawnBatch(request, count);
		}
	}

	private void SpawnPreSpawnEffecter(SpawnRequest request, int spawnsLeft)
	{
		for (int num = Mathf.Min(request.batchSize, spawnsLeft) - 1; num >= 0; num--)
		{
			IntVec3 target = request.spawnPositions[num];
			request.preSpawnEffect?.SpawnMaintained(target, map);
		}
	}

	private void SpawnBatch(SpawnRequest request, int spawnsLeft)
	{
		for (int num = Mathf.Min(request.batchSize, spawnsLeft) - 1; num >= 0; num--)
		{
			Thing thing = request.unspawnedThings[num];
			IntVec3 loc = request.spawnPositions[num];
			Thing thing2 = GenSpawn.Spawn(thing, loc, map, Rot4.Random);
			request.unspawnedThings.RemoveAt(num);
			request.spawnPositions.RemoveAt(num);
			request.spawnedThings.Add(thing);
			Pawn pawn = (thing2 as Pawn) ?? ((thing2 is IThingHolder thingHolder && thingHolder.GetDirectlyHeldThings().Count > 0) ? (thingHolder.GetDirectlyHeldThings()[0] as Pawn) : null);
			if (pawn != null)
			{
				if (request.lord != null)
				{
					pawn.GetLord()?.RemovePawn(pawn);
					request.lord?.AddPawn(pawn);
				}
				pawn.rotationTracker?.FaceCell(pawn.Map.Center);
			}
			request.spawnWorker?.OnSpawn(thing2);
			request.spawnSound?.PlayOneShot(thing2);
			request.spawnEffect?.SpawnMaintained(thing2, thing2.Map);
		}
		if (request.unspawnedThings.Count == 0)
		{
			request.done = true;
		}
	}

	public void Notify_MapRemoved()
	{
		foreach (SpawnRequest request in requests)
		{
			foreach (Thing unspawnedThing in request.unspawnedThings)
			{
				if (unspawnedThing is Pawn pawn)
				{
					Find.WorldPawns.PassToWorld(pawn);
				}
				ThingOwner thingOwner = (unspawnedThing as IThingHolder)?.GetDirectlyHeldThings();
				if (thingOwner != null && thingOwner.Count > 0 && thingOwner[0] is Pawn pawn2)
				{
					Find.WorldPawns.PassToWorld(pawn2);
				}
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref requests, "requests", LookMode.Deep);
	}
}
