using Verse;

namespace RimWorld;

public class QuestPart_PassWithFactionArg : QuestPart_Pass
{
	public Faction faction;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag == inSignal)
		{
			SignalArgs args = new SignalArgs(signal.args);
			if (outSignalOutcomeArg.HasValue)
			{
				args.Add(outSignalOutcomeArg.Value.Named("OUTCOME"));
			}
			args.Add(faction.Named("FACTION"));
			Find.SignalManager.SendSignal(new Signal(outSignal, args));
		}
	}

	public override void Notify_FactionRemoved(Faction f)
	{
		if (faction == f)
		{
			faction = null;
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref faction, "faction");
	}
}
