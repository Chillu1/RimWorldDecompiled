using Verse;

namespace RimWorld
{
	public abstract class QuestPartCanDisable : QuestPart
	{
		public string inSignalDisable;

		public QuestPartState state = QuestPartState.Enabled;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (state == QuestPartState.Enabled)
			{
				if (signal.tag == inSignalDisable)
				{
					Disable();
				}
				else
				{
					ProcessQuestSignal(signal);
				}
			}
		}

		protected virtual void ProcessQuestSignal(Signal signal)
		{
		}

		public void Disable()
		{
			state = QuestPartState.Disabled;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignalDisable, "inSignalDisable");
			Scribe_Values.Look(ref state, "state", QuestPartState.NeverEnabled);
		}
	}
}
