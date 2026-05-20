using Verse;

namespace RimWorld;

public abstract class CompRitualEffect_IntervalSpawn : RitualVisualEffectComp
{
	public int lastSpawnTick = -1;

	public int ticksPassed;

	protected int burstsDone;

	protected CompProperties_RitualEffectIntervalSpawn Props => (CompProperties_RitualEffectIntervalSpawn)props;

	public override bool ShouldSpawnNow(LordJob_Ritual ritual)
	{
		if (Props.delay > 0 && ticksPassed < Props.delay)
		{
			return false;
		}
		if (Props.maxBursts > 0 && burstsDone >= Props.maxBursts)
		{
			return false;
		}
		if (lastSpawnTick != -1)
		{
			return GenTicks.TicksGame - lastSpawnTick >= Props.spawnIntervalTicks;
		}
		return true;
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		ticksPassed += delta;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastSpawnTick, "lastSpawnTick", -1);
		Scribe_Values.Look(ref burstsDone, "burstsDone", 0);
		Scribe_Values.Look(ref ticksPassed, "ticksPassed", 0);
	}
}
