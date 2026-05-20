using Verse;
using Verse.Steam;

namespace RimWorld
{
	public static class VersionUpdateDialogMaker
	{
		private static bool dialogDone;

		public static void CreateVersionUpdateDialogIfNecessary()
		{
			if (!dialogDone && LastPlayedVersion.Version != null && (VersionControl.CurrentMajor != LastPlayedVersion.Version.Major || VersionControl.CurrentMinor != LastPlayedVersion.Version.Minor))
			{
				CreateNewVersionDialog();
			}
		}

		private static void CreateNewVersionDialog()
		{
			string text = LastPlayedVersion.Version.Major + "." + LastPlayedVersion.Version.Minor;
			string text2 = VersionControl.CurrentMajor + "." + VersionControl.CurrentMinor;
			string text3 = "GameUpdatedToNewVersionInitial".Translate(text, text2);
			text3 += "\n\n";
			text3 = ((!BackCompatibility.IsSaveCompatibleWith(LastPlayedVersion.Version.ToString())) ? ((string)(text3 + "GameUpdatedToNewVersionSavesIncompatible".Translate())) : ((string)(text3 + "GameUpdatedToNewVersionSavesCompatible".Translate())));
			text3 += "\n\n";
			text3 = ((!SteamDeck.IsSteamDeckInNonKeyboardMode) ? ((string)(text3 + "GameUpdatedToNewVersionSteam".Translate())) : ((string)(text3 + "GameUpdatedToNewVersionSteamController".Translate())));
			Find.WindowStack.Add(new Dialog_MessageBox(text3));
			dialogDone = true;
		}
	}
}
