using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_SpawnHateChantersDelayed : LordToil
{
	private readonly string readySignal;

	private readonly int batchSize;

	private readonly float spawnInterval;

	private readonly List<Pawn> pawns;

	private int spawnedCount;

	private bool spawnRequested;

	private SpawnRequest spawnRequest;

	public LordToil_SpawnHateChantersDelayed(List<Pawn> pawns, string readySignal, int batchSize, float spawnInterval)
	{
		this.pawns = pawns;
		this.readySignal = readySignal;
		this.batchSize = batchSize;
		this.spawnInterval = spawnInterval;
	}

	public override void Init()
	{
		base.Init();
		foreach (Pawn pawn in pawns)
		{
			if (pawn?.mindState != null)
			{
				pawn.mindState.duty = new PawnDuty(DutyDefOf.Idle);
				pawn.mindState.Active = false;
			}
		}
		spawnRequest = base.Map.deferredSpawner.GetRequestByLord(lord);
		if (spawnRequest == null)
		{
			spawnRequest = new SpawnRequest(pawns.Cast<Thing>().ToList(), batchSize, spawnInterval, lord);
			base.Map.deferredSpawner.AddRequest(spawnRequest);
		}
		else
		{
			spawnedCount = spawnRequest.spawnedThings.Count;
		}
		spawnRequested = true;
	}

	public override void UpdateAllDuties()
	{
	}

	public override void LordToilTick()
	{
		base.LordToilTick();
		if (!spawnRequested)
		{
			spawnRequest = base.Map.deferredSpawner.GetRequestByLord(lord);
			base.Map.deferredSpawner.AddRequest(spawnRequest);
			spawnRequested = true;
		}
		int count = spawnRequest.spawnedThings.Count;
		if (count > spawnedCount)
		{
			for (int i = spawnedCount; i < count; i++)
			{
				Hediff_DeathRefusal firstHediff = (spawnRequest.spawnedThings[i] as Pawn).health.hediffSet.GetFirstHediff<Hediff_DeathRefusal>();
				if (firstHediff != null)
				{
					firstHediff.AIEnabled = false;
				}
			}
			spawnedCount = count;
		}
		if (!spawnRequest.done)
		{
			return;
		}
		foreach (Pawn spawnedThing in spawnRequest.spawnedThings)
		{
			if (spawnedThing.mindState != null)
			{
				spawnedThing.mindState.Active = true;
				spawnedThing.mindState.duty = new PawnDuty(DutyDefOf.Idle);
			}
		}
		Find.SignalManager.SendSignal(new Signal(readySignal));
	}
}
