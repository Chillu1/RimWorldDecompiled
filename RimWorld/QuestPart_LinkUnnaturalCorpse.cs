using Verse;

namespace RimWorld;

public class QuestPart_LinkUnnaturalCorpse : QuestPart
{
	public Pawn aboutPawn;

	public UnnaturalCorpse corpse;

	public string inSignal;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignal && !aboutPawn.DestroyedOrNull())
		{
			Find.Anomaly.RegisterUnnaturalCorpse(aboutPawn, corpse);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref aboutPawn, "aboutPawn");
		Scribe_References.Look(ref corpse, "corpse");
		Scribe_Values.Look(ref inSignal, "inSignal");
	}
}
