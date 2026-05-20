using System;
using Verse;

namespace RimWorld
{
	public class Dialog_ModList_Save : Dialog_ModList
	{
		private ModList savingModList;

		private Action onClosed;

		protected override bool ShouldDoTypeInField => true;

		public Dialog_ModList_Save(ModList modList, Action onClosed = null)
		{
			interactButLabel = "OverwriteButton".Translate();
			savingModList = modList;
			this.onClosed = onClosed;
		}

		protected override void DoFileInteraction(string fileName)
		{
			fileName = GenFile.SanitizedFileName(fileName);
			string absPath = GenFilePaths.AbsFilePathForModList(fileName);
			LongEventHandler.QueueLongEvent(delegate
			{
				GameDataSaveLoader.SaveModList(savingModList, absPath);
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
