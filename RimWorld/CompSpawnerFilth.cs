using Verse;

namespace RimWorld;

public class CompSpawnerFilth : ThingComp
{
	private int nextSpawnTimestamp = -1;

	private CompProperties_SpawnerFilth Props => (CompProperties_SpawnerFilth)props;

	private bool CanSpawnFilth
	{
		get
		{
			if (parent is Hive hive && !hive.CompDormant.Awake)
			{
				return false;
			}
			if (Props.requiredRotStage.HasValue && parent.GetRotStage() != Props.requiredRotStage)
			{
				return false;
			}
			return true;
		}
	}

	public override void PostExposeData()
	{
		base.PostExposeData();
		Scribe_Values.Look(ref nextSpawnTimestamp, "nextSpawnTimestamp", -1);
	}

	public override void PostSpawnSetup(bool respawningAfterLoad)
	{
		if (!respawningAfterLoad)
		{
			for (int i = 0; i < Props.spawnCountOnSpawn; i++)
			{
				TrySpawnFilth();
			}
		}
	}

	public override void CompTickInterval(int delta)
	{
		base.CompTickInterval(delta);
		TickIntervalDelta(delta);
	}

	public override void CompTickRare()
	{
		base.CompTickRare();
		TickIntervalDelta(250);
	}

	private void TickIntervalDelta(int interval)
	{
		if (!CanSpawnFilth)
		{
			return;
		}
		if (Props.spawnMtbHours > 0f && Rand.MTBEventOccurs(Props.spawnMtbHours, 2500f, interval))
		{
			TrySpawnFilth();
		}
		if (Props.spawnEveryDays >= 0f && Find.TickManager.TicksGame >= nextSpawnTimestamp)
		{
			if (nextSpawnTimestamp != -1)
			{
				TrySpawnFilth();
			}
			nextSpawnTimestamp = Find.TickManager.TicksGame + (int)(Props.spawnEveryDays * 60000f);
		}
	}

	public void TrySpawnFilth()
	{
		if (parent.Map != null && CellFinder.TryFindRandomReachableNearbyCell(parent.Position, parent.Map, Props.spawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors), (IntVec3 x) => x.Standable(parent.Map), (Region x) => true, out var result))
		{
			FilthMaker.TryMakeFilth(result, parent.Map, Props.filthDef);
		}
	}
}
