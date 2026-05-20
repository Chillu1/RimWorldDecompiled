using Verse;

namespace RimWorld;

public class Dialog_SaveFileList_Load : Dialog_SaveFileList
{
	protected override bool FocusSearchField => true;

	public Dialog_SaveFileList_Load()
	{
		interactButLabel = "LoadGameButton".Translate();
	}

	protected override void DoFileInteraction(string saveFileName)
	{
		GameDataSaveLoader.CheckVersionAndLoadGame(saveFileName);
	}
}
