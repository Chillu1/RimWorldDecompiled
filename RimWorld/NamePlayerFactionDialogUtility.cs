using System.IO;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld;

public static class NamePlayerFactionDialogUtility
{
	public const int CharacterLimit = 64;

	public static bool IsValidName(string s)
	{
		if (s.Length == 0)
		{
			return false;
		}
		if (s.Length > 64)
		{
			return false;
		}
		if (!GenText.IsValidFilename(s))
		{
			return false;
		}
		if (GrammarResolver.ContainsSpecialChars(s))
		{
			return false;
		}
		return true;
	}

	public static void Named(string s)
	{
		Faction.OfPlayer.Name = s;
		if (!Find.GameInfo.permadeathMode)
		{
			return;
		}
		string oldSavefileName = Find.GameInfo.permadeathModeUniqueName;
		string newSavefileName = PermadeathModeUtility.GeneratePermadeathSaveNameBasedOnPlayerInput(s, oldSavefileName);
		if (!(oldSavefileName != newSavefileName))
		{
			return;
		}
		LongEventHandler.QueueLongEvent(delegate
		{
			Find.GameInfo.permadeathModeUniqueName = newSavefileName;
			Find.Autosaver.DoAutosave();
			GenFilePaths.AllSavedGameFiles.FirstOrDefault((FileInfo x) => Path.GetFileNameWithoutExtension(x.Name) == oldSavefileName)?.Delete();
		}, "Autosaving", doAsynchronously: false, null);
	}
}
