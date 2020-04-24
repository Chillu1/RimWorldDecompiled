using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Verse
{
	public class ScribeMetaHeaderUtility
	{
		public enum ScribeHeaderMode
		{
			None,
			Map,
			World,
			Scenario
		}

		private static ScribeHeaderMode lastMode;

		public static string loadedGameVersion;

		public static List<string> loadedModIdsList;

		public static List<string> loadedModNamesList;

		public const string MetaNodeName = "meta";

		public const string GameVersionNodeName = "gameVersion";

		public const string ModIdsNodeName = "modIds";

		public const string ModNamesNodeName = "modNames";

		public static void WriteMetaHeader()
		{
			if (Scribe.EnterNode("meta"))
			{
				try
				{
					string value = VersionControl.CurrentVersionStringWithRev;
					Scribe_Values.Look(ref value, "gameVersion");
					List<string> list = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.PackageId).ToList();
					Scribe_Collections.Look(ref list, "modIds", LookMode.Undefined);
					List<string> list2 = LoadedModManager.RunningMods.Select((ModContentPack mod) => mod.Name).ToList();
					Scribe_Collections.Look(ref list2, "modNames", LookMode.Undefined);
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
		}

		public static void LoadGameDataHeader(ScribeHeaderMode mode, bool logVersionConflictWarning)
		{
			loadedGameVersion = "Unknown";
			loadedModIdsList = null;
			loadedModNamesList = null;
			lastMode = mode;
			if (Scribe.mode != 0 && Scribe.EnterNode("meta"))
			{
				try
				{
					Scribe_Values.Look(ref loadedGameVersion, "gameVersion");
					Scribe_Collections.Look(ref loadedModIdsList, "modIds", LookMode.Undefined);
					Scribe_Collections.Look(ref loadedModNamesList, "modNames", LookMode.Undefined);
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			if (logVersionConflictWarning && (mode == ScribeHeaderMode.Map || !UnityData.isEditor) && !VersionsMatch())
			{
				Log.Warning("Loaded file (" + mode + ") is from version " + loadedGameVersion + ", we are running version " + VersionControl.CurrentVersionStringWithRev + ".");
			}
		}

		private static bool VersionsMatch()
		{
			return VersionControl.BuildFromVersionString(loadedGameVersion) == VersionControl.BuildFromVersionString(VersionControl.CurrentVersionStringWithRev);
		}

		public static bool TryCreateDialogsForVersionMismatchWarnings(Action confirmedAction)
		{
			string text = null;
			string text2 = null;
			if (!BackCompatibility.IsSaveCompatibleWith(loadedGameVersion) && !VersionsMatch())
			{
				text2 = "VersionMismatch".Translate();
				string value = loadedGameVersion.NullOrEmpty() ? ("(" + "UnknownLower".TranslateSimple() + ")") : loadedGameVersion;
				text = ((lastMode == ScribeHeaderMode.Map) ? ((string)"SaveGameIncompatibleWarningText".Translate(value, VersionControl.CurrentVersionString)) : ((lastMode != ScribeHeaderMode.World) ? ((string)"FileIncompatibleWarning".Translate(value, VersionControl.CurrentVersionString)) : ((string)"WorldFileVersionMismatch".Translate(value, VersionControl.CurrentVersionString))));
			}
			bool flag = false;
			if (!LoadedModsMatchesActiveMods(out string loadedModsSummary, out string runningModsSummary))
			{
				flag = true;
				string text3 = "ModsMismatchWarningText".Translate(loadedModsSummary, runningModsSummary);
				text = ((text != null) ? (text + "\n\n" + text3) : text3);
				if (text2 == null)
				{
					text2 = "ModsMismatchWarningTitle".Translate();
				}
			}
			if (text != null)
			{
				Dialog_MessageBox dialog = Dialog_MessageBox.CreateConfirmation(text, confirmedAction, destructive: false, text2);
				dialog.buttonAText = "LoadAnyway".Translate();
				if (flag)
				{
					dialog.buttonCText = "ChangeLoadedMods".Translate();
					dialog.buttonCAction = delegate
					{
						int num = ModLister.InstalledModsListHash(activeOnly: false);
						ModsConfig.SetActiveToList(loadedModIdsList);
						ModsConfig.Save();
						if (num == ModLister.InstalledModsListHash(activeOnly: false))
						{
							IEnumerable<string> items = from id in Enumerable.Range(0, loadedModIdsList.Count)
								where ModLister.GetModWithIdentifier(loadedModIdsList[id]) == null
								select loadedModNamesList[id];
							Messages.Message(string.Format("{0}: {1}", "MissingMods".Translate(), items.ToCommaList()), MessageTypeDefOf.RejectInput, historical: false);
							dialog.buttonCClose = false;
						}
						else
						{
							ModsConfig.RestartFromChangedMods();
						}
					};
				}
				Find.WindowStack.Add(dialog);
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
				if (a[i] != b[i])
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
				using (StreamReader input = new StreamReader(file.FullName))
				{
					using (XmlTextReader xmlTextReader = new XmlTextReader(input))
					{
						if (ReadToMetaElement(xmlTextReader) && xmlTextReader.ReadToDescendant("gameVersion"))
						{
							return VersionControl.VersionStringWithoutRev(xmlTextReader.ReadString());
						}
					}
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
}
