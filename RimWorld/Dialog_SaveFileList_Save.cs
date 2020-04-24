using Verse;

namespace RimWorld
{
	public class Dialog_SaveFileList_Save : Dialog_SaveFileList
	{
		protected override bool ShouldDoTypeInField => true;

		public Dialog_SaveFileList_Save()
		{
			interactButLabel = "OverwriteButton".Translate();
			bottomAreaHeight = 85f;
			if (Faction.OfPlayer.HasName)
			{
				typingName = Faction.OfPlayer.Name;
			}
			else
			{
				typingName = SaveGameFilesUtility.UnusedDefaultFileName(Faction.OfPlayer.def.LabelCap);
			}
		}

		protected override void DoFileInteraction(string mapName)
		{
			mapName = GenFile.SanitizedFileName(mapName);
			LongEventHandler.QueueLongEvent(delegate
			{
				GameDataSaveLoader.SaveGame(mapName);
			}, "SavingLongEvent", doAsynchronously: false, null);
			Messages.Message("SavedAs".Translate(mapName), MessageTypeDefOf.SilentInput, historical: false);
			PlayerKnowledgeDatabase.Save();
			Close();
		}
	}
}
