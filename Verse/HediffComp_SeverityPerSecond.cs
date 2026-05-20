namespace Verse;

public class HediffComp_SeverityPerSecond : HediffComp
{
	protected float severityPerSecond;

	private HediffCompProperties_SeverityPerSecond Props => (HediffCompProperties_SeverityPerSecond)props;

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		base.CompPostPostAdd(dinfo);
		severityPerSecond = Props.CalculateSeverityPerSecond();
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		severityAdjustment += severityPerSecond / 60f * (parent.CurStage?.severityGainFactor ?? 1f);
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref severityPerSecond, "severityPerDay", 0f);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && severityPerSecond == 0f && Props.severityPerSecond != 0f && Props.severityPerSecondRange == FloatRange.Zero)
		{
			severityPerSecond = Props.CalculateSeverityPerSecond();
			Log.Warning("Hediff " + parent.Label + " had severityPerSecond not matching parent.");
		}
	}
}
