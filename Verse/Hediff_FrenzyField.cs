namespace Verse;

public class Hediff_FrenzyField : Hediff
{
	public int lastTickInRangeOfInducer;

	private const float SeverityRaisePerTick = 0.000132f;

	private const float SeverityFallPerTick = 2.2400001E-05f;

	private const int InducerBufferTicks = 60;

	public bool InRangeOfInducer => GenTicks.TicksGame <= lastTickInRangeOfInducer + 60;

	public override bool ShouldRemove
	{
		get
		{
			if (!InRangeOfInducer)
			{
				return base.ShouldRemove;
			}
			return false;
		}
	}

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Frenzy field"))
		{
			pawn.health.RemoveHediff(this);
		}
		else
		{
			base.PostAdd(dinfo);
		}
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (InRangeOfInducer)
		{
			Severity += 0.000132f * (float)delta;
		}
		else
		{
			Severity -= 2.2400001E-05f * (float)delta;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastTickInRangeOfInducer, "lastTickInRangeOfInducer", 0);
	}
}
