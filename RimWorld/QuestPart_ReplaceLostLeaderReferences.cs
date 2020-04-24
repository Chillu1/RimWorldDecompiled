using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_ReplaceLostLeaderReferences : QuestPart
	{
		public string inSignal;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (!(signal.tag == inSignal))
			{
				return;
			}
			Pawn arg = signal.args.GetArg<Pawn>("SUBJECT");
			Pawn arg2 = signal.args.GetArg<Pawn>("NEWFACTIONLEADER");
			if (arg == null || arg2 == null)
			{
				return;
			}
			List<QuestPart> partsListForReading = quest.PartsListForReading;
			for (int i = 0; i < partsListForReading.Count; i++)
			{
				partsListForReading[i].ReplacePawnReferences(arg, arg2);
			}
			if (arg.questTags != null)
			{
				if (arg2.questTags == null)
				{
					arg2.questTags = new List<string>();
				}
				arg2.questTags.AddRange(arg.questTags);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
		}
	}
}
