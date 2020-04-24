using Verse;

namespace RimWorld
{
	public class Dialog_ScenarioList_Save : Dialog_ScenarioList
	{
		private Scenario savingScen;

		protected override bool ShouldDoTypeInField => true;

		public Dialog_ScenarioList_Save(Scenario scen)
		{
			interactButLabel = "OverwriteButton".Translate();
			typingName = scen.name;
			savingScen = scen;
		}

		protected override void DoFileInteraction(string fileName)
		{
			string absPath = GenFilePaths.AbsPathForScenario(fileName);
			LongEventHandler.QueueLongEvent(delegate
			{
				GameDataSaveLoader.SaveScenario(savingScen, absPath);
			}, "SavingLongEvent", doAsynchronously: false, null);
			Messages.Message("SavedAs".Translate(fileName), MessageTypeDefOf.SilentInput, historical: false);
			Close();
		}
	}
}
