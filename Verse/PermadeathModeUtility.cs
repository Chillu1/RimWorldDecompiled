using RimWorld;

namespace Verse
{
	public static class PermadeathModeUtility
	{
		public static string GeneratePermadeathSaveName()
		{
			return NewPermadeathSaveNameWithAppendedNumberIfNecessary(GenFile.SanitizedFileName(NameGenerator.GenerateName(Faction.OfPlayer.def.factionNameMaker)));
		}

		public static string GeneratePermadeathSaveNameBasedOnPlayerInput(string factionName, string acceptedNameEvenIfTaken = null)
		{
			return NewPermadeathSaveNameWithAppendedNumberIfNecessary(GenFile.SanitizedFileName(factionName), acceptedNameEvenIfTaken);
		}

		public static void CheckUpdatePermadeathModeUniqueNameOnGameLoad(string filename)
		{
			if (Current.Game.Info.permadeathMode && Current.Game.Info.permadeathModeUniqueName != filename)
			{
				Log.Warning("Savefile's name has changed and doesn't match permadeath mode's unique name. Fixing...");
				Current.Game.Info.permadeathModeUniqueName = filename;
			}
		}

		private static string NewPermadeathSaveNameWithAppendedNumberIfNecessary(string name, string acceptedNameEvenIfTaken = null)
		{
			int num = 0;
			string text;
			do
			{
				num++;
				text = name;
				if (num != 1)
				{
					text += num;
				}
				text = AppendedPermadeathModeSuffix(text);
			}
			while (SaveGameFilesUtility.SavedGameNamedExists(text) && text != acceptedNameEvenIfTaken);
			return text;
		}

		private static string AppendedPermadeathModeSuffix(string str)
		{
			return str + " " + "PermadeathModeSaveSuffix".Translate();
		}
	}
}
