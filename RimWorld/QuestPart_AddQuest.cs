using RimWorld.QuestGen;
using Verse;

namespace RimWorld
{
	public abstract class QuestPart_AddQuest : QuestPart
	{
		public string inSignal;

		public Pawn acceptee;

		public QuestScriptDef questDef;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				QuestUtility.GenerateQuestAndMakeAvailable(questDef, GetSlate()).Accept(acceptee);
			}
		}

		public abstract Slate GetSlate();

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_References.Look(ref acceptee, "acceptee");
			Scribe_Defs.Look(ref questDef, "questToAdd");
		}
	}
}
