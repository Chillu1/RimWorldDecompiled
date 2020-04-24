using Verse;

namespace RimWorld
{
	public class QuestPart_GiveTechprints : QuestPart
	{
		public const string WasGivenSignal = "AddedTechprints";

		public string inSignal;

		public string outSignalWasGiven;

		public ResearchProjectDef project;

		public int amount;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				for (int i = 0; i < amount; i++)
				{
					Find.ResearchManager.ApplyTechprint(project, null);
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref project, "project");
			Scribe_Values.Look(ref amount, "amount", 0);
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref outSignalWasGiven, "outSignalWasGiven");
		}
	}
}
