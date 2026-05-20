using System.IO;
using System.Linq;

namespace Verse;

public static class SaveGameFilesUtility
{
	public static bool IsAutoSave(string fileName)
	{
		if (fileName.Length < 8)
		{
			return false;
		}
		return fileName.Substring(0, 8) == "Autosave";
	}

	public static bool SavedGameNamedExists(string fileName)
	{
		foreach (string item in GenFilePaths.AllSavedGameFiles.Select((FileInfo f) => Path.GetFileNameWithoutExtension(f.Name)))
		{
			if (item == fileName)
			{
				return true;
			}
		}
		return false;
	}

	public static string UnusedDefaultFileName(string factionLabel)
	{
		string text = "";
		int num = 1;
		do
		{
			text = factionLabel + num;
			num++;
		}
		while (SavedGameNamedExists(text));
		return text;
	}

	public static FileInfo GetAutostartSaveFile()
	{
		if (!Prefs.DevMode)
		{
			return null;
		}
		return GenFilePaths.AllSavedGameFiles.FirstOrDefault((FileInfo x) => Path.GetFileNameWithoutExtension(x.Name).ToLower() == "autostart".ToLower());
	}
}
