using Verse;

namespace RimWorld
{
	public class Dialog_SaveFileList_Load : Dialog_SaveFileList
	{
		public Dialog_SaveFileList_Load()
		{
			interactButLabel = "LoadGameButton".Translate();
		}

		protected override void DoFileInteraction(string saveFileName)
		{
			GameDataSaveLoader.CheckVersionAndLoadGame(saveFileName);
		}
	}
}
