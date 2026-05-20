using Verse;

namespace RimWorld;

public class QuestPart_GiveHediff : QuestPart
{
	public Pawn aboutPawn;

	public string inSignal;

	public HediffDef hediff;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignal && !aboutPawn.DestroyedOrNull())
		{
			aboutPawn.health.AddHediff(hediff);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref aboutPawn, "aboutPawn");
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Defs.Look(ref hediff, "hediff");
	}
}
