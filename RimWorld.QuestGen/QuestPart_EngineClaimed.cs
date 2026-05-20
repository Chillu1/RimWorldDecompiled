using Verse;

namespace RimWorld.QuestGen;

public class QuestPart_EngineClaimed : QuestPart
{
	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (signal.tag.EndsWith("ChangedFactionToPlayer") && signal.args.TryGetArg("SUBJECT", out var arg) && arg.arg is Thing thing && thing.def == ThingDefOf.GravEngine)
		{
			quest.End(QuestEndOutcome.Success);
		}
	}
}
