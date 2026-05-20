using Verse;

namespace RimWorld
{
	public class Dialog_IdeoList_Save : Dialog_IdeoList
	{
		private Ideo savingIdeo;

		protected override bool ShouldDoTypeInField => true;

		public Dialog_IdeoList_Save(Ideo ideo)
		{
			interactButLabel = "OverwriteButton".Translate();
			typingName = ideo.name;
			savingIdeo = ideo;
		}

		protected override void DoFileInteraction(string fileName)
		{
			fileName = GenFile.SanitizedFileName(fileName);
			string absPath = GenFilePaths.AbsPathForIdeo(fileName);
			LongEventHandler.QueueLongEvent(delegate
			{
				GameDataSaveLoader.SaveIdeo(savingIdeo, absPath);
			}, "SavingLongEvent", doAsynchronously: false, null);
			Messages.Message("SavedAs".Translate(fileName), MessageTypeDefOf.SilentInput, historical: false);
			Close();
		}
	}
}
