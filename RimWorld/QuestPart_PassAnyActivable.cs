using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_PassAnyActivable : QuestPartActivable
	{
		public List<string> inSignals = new List<string>();

		protected override void ProcessQuestSignal(Signal signal)
		{
			base.ProcessQuestSignal(signal);
			if (inSignals.Contains(signal.tag))
			{
				Complete();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref inSignals, "inSignals", LookMode.Value);
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignals.Clear();
			for (int i = 0; i < 3; i++)
			{
				inSignals.Add("DebugSignal" + Rand.Int);
			}
		}
	}
}
