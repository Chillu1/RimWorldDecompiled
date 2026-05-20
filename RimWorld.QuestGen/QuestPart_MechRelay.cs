using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_MechRelay : QuestPart
{
	public string inSignal;

	public Thing relay;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignal)
		{
			relay?.TryGetComp<CompMechRelay>()?.Deactivate();
		}
	}

	public override void ExposeData()
	{
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_References.Look(ref relay, "relay");
	}
}
