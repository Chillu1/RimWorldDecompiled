using Verse;

namespace RimWorld
{
	public class QuestPart_PassActivable : QuestPartActivable
	{
		public string inSignal;

		protected override void ProcessQuestSignal(Signal signal)
		{
			base.ProcessQuestSignal(signal);
			if (signal.tag == inSignal)
			{
				Complete(signal.args);
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
