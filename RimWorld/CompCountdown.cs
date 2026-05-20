using Verse;

namespace RimWorld;

public class CompCountdown : ThingComp
{
	public int endTick;

	public CompProperties_Countdown Props => (CompProperties_Countdown)props;

	public override string CompInspectStringExtra()
	{
		int num = endTick - GenTicks.TicksGame;
		if (num <= 0)
		{
			return null;
		}
		return Props.label.Formatted(num.ToStringTicksToPeriod().Colorize(ColoredText.DateTimeColor).Named("DURATION")).Resolve();
	}

	public override void PostExposeData()
	{
		Scribe_Values.Look(ref endTick, "endTick", 0);
	}
}
