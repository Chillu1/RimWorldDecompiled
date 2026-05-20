using Verse;

namespace RimWorld
{
	public class QuestPart_PassWhileActive : QuestPartActivable
	{
		public string inSignal;

		public string outSignal;

		protected override void ProcessQuestSignal(Signal signal)
		{
			base.ProcessQuestSignal(signal);
			if (signal.tag == inSignal)
			{
				Find.SignalManager.SendSignal(new Signal(outSignal, signal.args));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref outSignal, "outSignal");
		}
	}
}
