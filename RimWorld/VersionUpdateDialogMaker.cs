using Verse;

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
			string value = LastPlayedVersion.Version.Major + "." + LastPlayedVersion.Version.Minor;
			string value2 = VersionControl.CurrentMajor + "." + VersionControl.CurrentMinor;
			string str = "GameUpdatedToNewVersionInitial".Translate(value, value2);
			str += "\n\n";
			str = ((!BackCompatibility.IsSaveCompatibleWith(LastPlayedVersion.Version.ToString())) ? ((string)(str + "GameUpdatedToNewVersionSavesIncompatible".Translate())) : ((string)(str + "GameUpdatedToNewVersionSavesCompatible".Translate())));
			str += "\n\n";
			str += "GameUpdatedToNewVersionSteam".Translate();
			Find.WindowStack.Add(new Dialog_MessageBox(str));
			dialogDone = true;
		}
	}
}
