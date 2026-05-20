using System;
using Verse;

namespace RimWorld
{
	public class Dialog_ScenarioList_Save : Dialog_ScenarioList
	{
		private Scenario savingScen;

		private Action onClosed;

		protected override bool ShouldDoTypeInField => true;

		public Dialog_ScenarioList_Save(Scenario scen, Action onClosed = null)
		{
			interactButLabel = "OverwriteButton".Translate();
			typingName = scen.name?.Trim();
			savingScen = scen;
			this.onClosed = onClosed;
		}

		protected override void DoFileInteraction(string fileName)
		{
			fileName = GenFile.SanitizedFileName(fileName);
			string absPath = GenFilePaths.AbsPathForScenario(fileName);
			LongEventHandler.QueueLongEvent(delegate
			{
				GameDataSaveLoader.SaveScenario(savingScen, absPath);
			}, "SavingLongEvent", doAsynchronously: false, null);
			Messages.Message("SavedAs".Translate(fileName), MessageTypeDefOf.SilentInput, historical: false);
			Close();
		}

		public override void Close(bool doCloseSound = true)
		{
			base.Close(doCloseSound);
			onClosed?.Invoke();
		}
	}
}
