using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GenFilePaths
	{
		private static string saveDataPath = null;

		private static string modsFolderPath = null;

		private static string officialModsFolderPath = null;

		public const string SoundsFolder = "Sounds/";

		public const string SoundsFolderName = "Sounds";

		public const string TexturesFolder = "Textures/";

		public const string TexturesFolderName = "Textures";

		public const string StringsFolder = "Strings/";

		public const string DefsFolder = "Defs/";

		public const string PatchesFolder = "Patches/";

		public const string AssetBundlesFolderName = "AssetBundles";

		public const string AssetsFolderName = "Assets";

		public const string ResourcesFolderName = "Resources";

		public const string ModsFolderName = "Mods";

		public const string AssembliesFolder = "Assemblies/";

		public const string OfficialModsFolderName = "Data";

		public const string CoreFolderName = "Core";

		public const string BackstoriesPath = "Backstories";

		public const string SavedGameExtension = ".rws";

		public const string ScenarioExtension = ".rsc";

		public const string ExternalHistoryFileExtension = ".rwh";

		private const string SaveDataFolderCommand = "savedatafolder";

		private static readonly string[] FilePathRaw = new string[73]
		{
			"Ž",
			"ž",
			"Ÿ",
			"¡",
			"¢",
			"£",
			"¤",
			"¥",
			"¦",
			"§",
			"\u00a8",
			"©",
			"ª",
			"À",
			"Á",
			"Â",
			"Ã",
			"Ä",
			"Å",
			"Æ",
			"Ç",
			"È",
			"É",
			"Ê",
			"Ë",
			"Ì",
			"Í",
			"Î",
			"Ï",
			"Ð",
			"Ñ",
			"Ò",
			"Ó",
			"Ô",
			"Õ",
			"Ö",
			"Ù",
			"Ú",
			"Û",
			"Ü",
			"Ý",
			"Þ",
			"ß",
			"à",
			"á",
			"â",
			"ã",
			"ä",
			"å",
			"æ",
			"ç",
			"è",
			"é",
			"ê",
			"ë",
			"ì",
			"í",
			"î",
			"ï",
			"ð",
			"ñ",
			"ò",
			"ó",
			"ô",
			"õ",
			"ö",
			"ù",
			"ú",
			"û",
			"ü",
			"ý",
			"þ",
			"ÿ"
		};

		private static readonly string[] FilePathSafe = new string[73]
		{
			"%8E",
			"%9E",
			"%9F",
			"%A1",
			"%A2",
			"%A3",
			"%A4",
			"%A5",
			"%A6",
			"%A7",
			"%A8",
			"%A9",
			"%AA",
			"%C0",
			"%C1",
			"%C2",
			"%C3",
			"%C4",
			"%C5",
			"%C6",
			"%C7",
			"%C8",
			"%C9",
			"%CA",
			"%CB",
			"%CC",
			"%CD",
			"%CE",
			"%CF",
			"%D0",
			"%D1",
			"%D2",
			"%D3",
			"%D4",
			"%D5",
			"%D6",
			"%D9",
			"%DA",
			"%DB",
			"%DC",
			"%DD",
			"%DE",
			"%DF",
			"%E0",
			"%E1",
			"%E2",
			"%E3",
			"%E4",
			"%E5",
			"%E6",
			"%E7",
			"%E8",
			"%E9",
			"%EA",
			"%EB",
			"%EC",
			"%ED",
			"%EE",
			"%EF",
			"%F0",
			"%F1",
			"%F2",
			"%F3",
			"%F4",
			"%F5",
			"%F6",
			"%F9",
			"%FA",
			"%FB",
			"%FC",
			"%FD",
			"%FE",
			"%FF"
		};

		public static string SaveDataFolderPath
		{
			get
			{
				if (saveDataPath == null)
				{
					if (GenCommandLine.TryGetCommandLineArg("savedatafolder", out string value))
					{
						value.TrimEnd('\\', '/');
						if (value == "")
						{
							value = (Path.DirectorySeparatorChar.ToString() ?? "");
						}
						saveDataPath = value;
						Log.Message("Save data folder overridden to " + saveDataPath);
					}
					else
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(UnityData.dataPath);
						if (UnityData.isEditor)
						{
							saveDataPath = Path.Combine(directoryInfo.Parent.ToString(), "SaveData");
						}
						else if (UnityData.platform == RuntimePlatform.OSXPlayer || UnityData.platform == RuntimePlatform.OSXEditor)
						{
							string path = Path.Combine(Directory.GetParent(UnityData.persistentDataPath).ToString(), "RimWorld");
							if (!Directory.Exists(path))
							{
								Directory.CreateDirectory(path);
							}
							saveDataPath = path;
						}
						else
						{
							saveDataPath = Application.persistentDataPath;
						}
					}
					DirectoryInfo directoryInfo2 = new DirectoryInfo(saveDataPath);
					if (!directoryInfo2.Exists)
					{
						directoryInfo2.Create();
					}
				}
				return saveDataPath;
			}
		}

		public static string ScenarioPreviewImagePath
		{
			get
			{
				if (!UnityData.isEditor)
				{
					return Path.Combine(ExecutableDir.FullName, "ScenarioPreview.jpg");
				}
				return Path.Combine(Path.Combine(Path.Combine(ExecutableDir.FullName, "PlatformSpecific"), "All"), "ScenarioPreview.jpg");
			}
		}

		private static DirectoryInfo ExecutableDir => new DirectoryInfo(UnityData.dataPath).Parent;

		public static string ModsFolderPath
		{
			get
			{
				if (modsFolderPath == null)
				{
					modsFolderPath = GetOrCreateModsFolder("Mods");
				}
				return modsFolderPath;
			}
		}

		public static string OfficialModsFolderPath
		{
			get
			{
				if (officialModsFolderPath == null)
				{
					officialModsFolderPath = GetOrCreateModsFolder("Data");
				}
				return officialModsFolderPath;
			}
		}

		public static string ConfigFolderPath => FolderUnderSaveData("Config");

		private static string SavedGamesFolderPath => FolderUnderSaveData("Saves");

		private static string ScenariosFolderPath => FolderUnderSaveData("Scenarios");

		private static string ExternalHistoryFolderPath => FolderUnderSaveData("External");

		public static string ScreenshotFolderPath => FolderUnderSaveData("Screenshots");

		public static string DevOutputFolderPath => FolderUnderSaveData("DevOutput");

		public static string ModsConfigFilePath => Path.Combine(ConfigFolderPath, "ModsConfig.xml");

		public static string ConceptKnowledgeFilePath => Path.Combine(ConfigFolderPath, "Knowledge.xml");

		public static string PrefsFilePath => Path.Combine(ConfigFolderPath, "Prefs.xml");

		public static string KeyPrefsFilePath => Path.Combine(ConfigFolderPath, "KeyPrefs.xml");

		public static string LastPlayedVersionFilePath => Path.Combine(ConfigFolderPath, "LastPlayedVersion.txt");

		public static string DevModePermanentlyDisabledFilePath => Path.Combine(ConfigFolderPath, "DevModeDisabled");

		public static string BackstoryOutputFilePath => Path.Combine(DevOutputFolderPath, "Fresh_Backstories.xml");

		public static string TempFolderPath => Application.temporaryCachePath;

		public static IEnumerable<FileInfo> AllSavedGameFiles
		{
			get
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(SavedGamesFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				return from f in directoryInfo.GetFiles()
					where f.Extension == ".rws"
					orderby f.LastWriteTime descending
					select f;
			}
		}

		public static IEnumerable<FileInfo> AllCustomScenarioFiles
		{
			get
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(ScenariosFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				return from f in directoryInfo.GetFiles()
					where f.Extension == ".rsc"
					orderby f.LastWriteTime descending
					select f;
			}
		}

		public static IEnumerable<FileInfo> AllExternalHistoryFiles
		{
			get
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(ExternalHistoryFolderPath);
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				return from f in directoryInfo.GetFiles()
					where f.Extension == ".rwh"
					orderby f.LastWriteTime descending
					select f;
			}
		}

		private static string FolderUnderSaveData(string folderName)
		{
			string text = Path.Combine(SaveDataFolderPath, folderName);
			DirectoryInfo directoryInfo = new DirectoryInfo(text);
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			return text;
		}

		public static string FilePathForSavedGame(string gameName)
		{
			return Path.Combine(SavedGamesFolderPath, gameName + ".rws");
		}

		public static string AbsPathForScenario(string scenarioName)
		{
			return Path.Combine(ScenariosFolderPath, scenarioName + ".rsc");
		}

		public static string ContentPath<T>()
		{
			if (typeof(T) == typeof(AudioClip))
			{
				return "Sounds/";
			}
			if (typeof(T) == typeof(Texture2D))
			{
				return "Textures/";
			}
			if (typeof(T) == typeof(string))
			{
				return "Strings/";
			}
			throw new ArgumentException();
		}

		private static string GetOrCreateModsFolder(string folderName)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(UnityData.dataPath);
			DirectoryInfo directoryInfo2 = (!UnityData.isEditor) ? directoryInfo.Parent : directoryInfo;
			string text = Path.Combine(directoryInfo2.ToString(), folderName);
			DirectoryInfo directoryInfo3 = new DirectoryInfo(text);
			if (!directoryInfo3.Exists)
			{
				directoryInfo3.Create();
			}
			return text;
		}

		public static string SafeURIForUnityWWWFromPath(string rawPath)
		{
			string text = rawPath;
			for (int i = 0; i < FilePathRaw.Length; i++)
			{
				text = text.Replace(FilePathRaw[i], FilePathSafe[i]);
			}
			return "file:///" + text;
		}
	}
}
