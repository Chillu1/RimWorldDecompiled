namespace Verse;

public abstract class HediffComp_Randomizer : HediffComp
{
	private int nextRandomizationTick;

	private HediffCompProperties_Randomizer Props => (HediffCompProperties_Randomizer)props;

	public abstract void Randomize();

	public override void CompPostMake()
	{
		SetNextRandomizationTick();
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (Find.TickManager.TicksGame >= nextRandomizationTick)
		{
			Randomize();
			SetNextRandomizationTick();
		}
	}

	public void SetNextRandomizationTick()
	{
		nextRandomizationTick = Find.TickManager.TicksGame + Props.ticksToRandomize.RandomInRange;
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref nextRandomizationTick, "nextRandomizationTick", 0);
	}

	public override string CompDebugString()
	{
		return $"ticks until randomization: {nextRandomizationTick - Find.TickManager.TicksGame}";
	}
}
