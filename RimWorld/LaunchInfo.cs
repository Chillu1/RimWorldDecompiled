using Verse;

namespace RimWorld;

public class LaunchInfo : IExposable
{
	public Pawn pilot;

	public Pawn copilot;

	public float quality;

	public bool doNegativeOutcome;

	public void ExposeData()
	{
		Scribe_References.Look(ref pilot, "pilot");
		Scribe_References.Look(ref copilot, "copilot");
		Scribe_Values.Look(ref quality, "quality", 0f);
		Scribe_Values.Look(ref doNegativeOutcome, "doNegativeOutcome", defaultValue: false);
	}
}
