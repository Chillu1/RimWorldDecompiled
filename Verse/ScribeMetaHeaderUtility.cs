using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using RimWorld;

namespace Verse;

public class ScribeMetaHeaderUtility
{
	public enum ScribeHeaderMode
	{
		None,
		Map,
		World,
		Scenario,
		Ideo,
		Xenotype,
		Xenogerm,
		ModList,
		CameraConfig
	}

	private static ScribeHeaderMode lastMode;

	public static string loadedGameVersion;

	public static int loadedGameVersionMajor;

	public static int loadedGameVersionMinor;

	public static int loadedGameVersionBuild;

	public static List<string> loadedModIdsList;

	public static List<string> loadedModNamesList;

	public static List<int> loadedModSteamIdsList;

	public static bool modListChanged;

	public const string MetaNodeName = "meta";

	public const string GameVersionNodeName = "gameVersion";

	public const string ModIdsNodeName = "modIds";

	public const string ModNamesNodeName = "modNames";

	public const string ModSteamIdsNodeName = "modSteamIds";

	public static void WriteMetaHeader()
	{
		if (!Scribe.EnterNode("meta"))
		{
			return;
		}
		try
		{
			string value = VersionControl.CurrentVersionStringWithRev;
			Scribe_Values.Look(ref value, "gameVersion");
			List<string> list = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageId).ToList();
			Scribe_Collections.Look(ref list, "modIds", LookMode.Undefined);
			List<uint> list2 = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.SteamAppId).ToList();
			Scribe_Collections.Look(ref list2, "modSteamIds", LookMode.Undefined);
			List<string> list3 = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.Name).ToList();
			Scribe_Collections.Look(ref list3, "modNames", LookMode.Undefined);
		}
		finally
		{
			Scribe.ExitNode();
		}
	}

	public static void LoadGameDataHeader(ScribeHeaderMode mode, bool logVersionConflictWarning)
	{
		loadedGameVersion = "Unknown";
		loadedGameVersionMajor = 0;
		loadedGameVersionMinor = 0;
		loadedGameVersionBuild = 0;
		loadedModIdsList = null;
		loadedModNamesList = null;
		modListChanged = false;
		lastMode = mode;
		if (Scribe.mode != LoadSaveMode.Inactive && Scribe.EnterNode("meta"))
		{
			try
			{
				Scribe_Values.Look(ref loadedGameVersion, "gameVersion");
				Scribe_Collections.Look(ref loadedModIdsList, "modIds", LookMode.Undefined);
				Scribe_Collections.Look(ref loadedModNamesList, "modNames", LookMode.Undefined);
				if (Scribe.mode == LoadSaveMode.LoadingVars && !loadedGameVersion.NullOrEmpty())
				{
					try
					{
						loadedGameVersionMajor = VersionControl.MajorFromVersionString(loadedGameVersion);
						loadedGameVersionMinor = VersionControl.MinorFromVersionString(loadedGameVersion);
						loadedGameVersionBuild = VersionControl.BuildFromVersionString(loadedGameVersion);
					}
					catch (Exception ex)
					{
						Log.Error("Error parsing loaded version. " + ex);
					}
				}
			}
			finally
			{
				Scribe.ExitNode();
			}
		}
		modListChanged = !LoadedModsMatchesActiveModsNoInfo();
		if (logVersionConflictWarning && (mode == ScribeHeaderMode.Map || !UnityData.isEditor) && !VersionsMatch())
		{
			Log.Warning("Loaded file (" + mode.ToString() + ") is from version " + loadedGameVersion + ", we are running version " + VersionControl.CurrentVersionStringWithRev + ".");
		}
	}

	private static bool VersionsMatch()
	{
		return loadedGameVersionBuild == VersionControl.BuildFromVersionString(VersionControl.CurrentVersionStringWithRev);
	}

	public static bool TryCreateDialogsForVersionMismatchWarnings(Action confirmedAction)
	{
		string text = null;
		string title = null;
		if (!BackCompatibility.IsSaveCompatibleWith(loadedGameVersion) && !VersionsMatch())
		{
			title = "VersionMismatch".Translate();
			string text2 = (loadedGameVersion.NullOrEmpty() ? ("(" + "UnknownLower".TranslateSimple() + ")") : loadedGameVersion);
			text = ((lastMode == ScribeHeaderMode.Map) ? ((string)"SaveGameIncompatibleWarningText".Translate(text2, VersionControl.CurrentVersionString)) : ((lastMode != ScribeHeaderMode.World) ? ((string)"FileIncompatibleWarning".Translate(text2, VersionControl.CurrentVersionString)) : ((string)"WorldFileVersionMismatch".Translate(text2, VersionControl.CurrentVersionString))));
		}
		if (!LoadedModsMatchesActiveMods(out var _, out var _))
		{
			Find.WindowStack.Add(new Dialog_ModMismatch(confirmedAction, loadedModIdsList, loadedModNamesList));
			return true;
		}
		if (text != null)
		{
			Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation(text, confirmedAction, destructive: false, title);
			dialog_MessageBox.buttonAText = "LoadAnyway".Translate();
			Find.WindowStack.Add(dialog_MessageBox);
			return true;
		}
		return false;
	}

	public static bool LoadedModsMatchesActiveMods(out string loadedModsSummary, out string runningModsSummary)
	{
		loadedModsSummary = null;
		runningModsSummary = null;
		List<string> list = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageId).ToList();
		List<string> b = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.FolderName).ToList();
		if (ModListsMatch(loadedModIdsList, list) || ModListsMatch(loadedModIdsList, b))
		{
			return true;
		}
		if (loadedModNamesList == null)
		{
			loadedModsSummary = "None".Translate();
		}
		else
		{
			loadedModsSummary = loadedModNamesList.ToCommaList();
		}
		runningModsSummary = list.Select((string id) => ModLister.GetModWithIdentifier(id).Name).ToCommaList();
		return false;
	}

	public static bool LoadedModsMatchesActiveModsNoInfo()
	{
		List<string> b = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageId).ToList();
		if (ModListsMatch(loadedModIdsList, b))
		{
			return true;
		}
		List<string> b2 = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.FolderName).ToList();
		return ModListsMatch(loadedModIdsList, b2);
	}

	private static bool ModListsMatch(List<string> a, List<string> b)
	{
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Count != b.Count)
		{
			return false;
		}
		for (int i = 0; i < a.Count; i++)
		{
			if (a[i].ToLower() != b[i].ToLower())
			{
				return false;
			}
		}
		return true;
	}

	public static string GameVersionOf(FileInfo file)
	{
		if (!file.Exists)
		{
			throw new ArgumentException();
		}
		try
		{
			using StreamReader input = new StreamReader(file.FullName);
			using XmlTextReader xmlTextReader = new XmlTextReader(input);
			if (ReadToMetaElement(xmlTextReader) && xmlTextReader.ReadToDescendant("gameVersion"))
			{
				return VersionControl.VersionStringWithoutRev(xmlTextReader.ReadString());
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception getting game version of " + file.Name + ": " + ex.ToString());
		}
		return null;
	}

	public static bool ReadToMetaElement(XmlTextReader textReader)
	{
		if (!ReadToNextElement(textReader))
		{
			return false;
		}
		if (!ReadToNextElement(textReader))
		{
			return false;
		}
		if (textReader.Name != "meta")
		{
			return false;
		}
		return true;
	}

	private static bool ReadToNextElement(XmlTextReader textReader)
	{
		do
		{
			if (!textReader.Read())
			{
				return false;
			}
		}
		while (textReader.NodeType != XmlNodeType.Element);
		return true;
	}
}
