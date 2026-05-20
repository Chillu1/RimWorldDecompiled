using RimWorld;

namespace Verse;

public class HediffComp_KillAfterDays : HediffComp
{
	private int ticksLeft;

	public HediffCompProperties_KillAfterDays Props => (HediffCompProperties_KillAfterDays)props;

	public override string CompTipStringExtra
	{
		get
		{
			if (ticksLeft <= 0)
			{
				return null;
			}
			return "DeathIn".Translate(ticksLeft.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor)).Resolve().CapitalizeFirst();
		}
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		ticksLeft = 60000 * Props.days;
	}

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		ticksLeft -= delta;
		if (ticksLeft <= 0)
		{
			base.Pawn.Kill(null, parent);
		}
	}

	public override void CompExposeData()
	{
		Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
	}
}
