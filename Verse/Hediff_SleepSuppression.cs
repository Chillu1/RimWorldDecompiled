namespace Verse;

public class Hediff_SleepSuppression : Hediff
{
	public int lastTickInRangeOfSuppressor;

	private const float FallRatePerHour = 0.33f;

	private const float GainRatePerHour = 0.16f;

	private const int SuppressorBufferTicks = 60;

	public bool InRangeOfSuppressor => GenTicks.TicksGame <= lastTickInRangeOfSuppressor + 60;

	public override bool ShouldRemove
	{
		get
		{
			if (!InRangeOfSuppressor)
			{
				return base.ShouldRemove;
			}
			return false;
		}
	}

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Sleep suppression"))
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
		if (!InRangeOfSuppressor)
		{
			Severity -= 0.000132f * (float)delta;
		}
		else
		{
			Severity += 6.4E-05f * (float)delta;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastTickInRangeOfSuppressor, "lastTickInRangeOfSuppressor", 0);
	}
}
