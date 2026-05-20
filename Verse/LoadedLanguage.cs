using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld.IO;
using UnityEngine;

namespace Verse;

public class LoadedLanguage
{
	public class KeyedReplacement
	{
		public string key;

		public string value;

		public string fileSource;

		public int fileSourceLine;

		public string fileSourceFullPath;

		public bool isPlaceholder;
	}

	public string folderName;

	public LanguageInfo info;

	private LanguageWorker workerInt;

	private LanguageWordInfo wordInfo = new LanguageWordInfo();

	private bool dataIsLoaded;

	public List<string> loadErrors = new List<string>();

	public bool anyKeyedReplacementsXmlParseError;

	public string lastKeyedReplacementsXmlParseErrorInFile;

	public bool anyDefInjectionsXmlParseError;

	public string lastDefInjectionsXmlParseErrorInFile;

	public bool anyError;

	private string legacyFolderName;

	private Dictionary<ModContentPack, HashSet<string>> tmpAlreadyLoadedFiles = new Dictionary<ModContentPack, HashSet<string>>();

	public Texture2D icon = BaseContent.BadTex;

	public Dictionary<string, KeyedReplacement> keyedReplacements = new Dictionary<string, KeyedReplacement>();

	public List<DefInjectionPackage> defInjections = new List<DefInjectionPackage>();

	public Dictionary<string, List<string>> stringFiles = new Dictionary<string, List<string>>();

	public const string OldKeyedTranslationsFolderName = "CodeLinked";

	public const string KeyedTranslationsFolderName = "Keyed";

	public const string OldDefInjectionsFolderName = "DefLinked";

	public const string DefInjectionsFolderName = "DefInjected";

	public const string LanguagesFolderName = "Languages";

	public const string PlaceholderText = "TODO";

	private bool infoIsRealMetadata;

	public string DisplayName => GenText.SplitCamelCase(folderName);

	public string FriendlyNameNative
	{
		get
		{
			if (info == null || info.friendlyNameNative.NullOrEmpty())
			{
				return folderName;
			}
			return info.friendlyNameNative;
		}
	}

	public string FriendlyNameEnglish
	{
		get
		{
			if (info == null || info.friendlyNameEnglish.NullOrEmpty())
			{
				return folderName;
			}
			return info.friendlyNameEnglish;
		}
	}

	public IEnumerable<Tuple<VirtualDirectory, ModContentPack, string>> AllDirectories
	{
		get
		{
			foreach (ModContentPack mod in LoadedModManager.RunningMods)
			{
				foreach (string item in mod.foldersToLoadDescendingOrder)
				{
					string path = Path.Combine(item, "Languages");
					VirtualDirectory directory = AbstractFilesystem.GetDirectory(Path.Combine(path, folderName));
					if (directory.Exists)
					{
						yield return new Tuple<VirtualDirectory, ModContentPack, string>(directory, mod, item);
						continue;
					}
					directory = AbstractFilesystem.GetDirectory(Path.Combine(path, legacyFolderName));
					if (directory.Exists)
					{
						yield return new Tuple<VirtualDirectory, ModContentPack, string>(directory, mod, item);
					}
				}
			}
		}
	}

	public LanguageWorker Worker
	{
		get
		{
			if (workerInt == null)
			{
				workerInt = (LanguageWorker)Activator.CreateInstance(info.languageWorkerClass);
			}
			return workerInt;
		}
	}

	public LanguageWordInfo WordInfo => wordInfo;

	public string LegacyFolderName => legacyFolderName;

	public LoadedLanguage(string folderName)
	{
		this.folderName = folderName;
		legacyFolderName = (folderName.Contains("(") ? folderName.Substring(0, folderName.IndexOf("(") - 1) : folderName).Trim();
	}

	public void LoadMetadata()
	{
		if (info != null && infoIsRealMetadata)
		{
			return;
		}
		infoIsRealMetadata = true;
		foreach (ModContentPack runningMod in LoadedModManager.RunningMods)
		{
			foreach (string item in runningMod.foldersToLoadDescendingOrder)
			{
				string text = Path.Combine(item, "Languages");
				if (!new DirectoryInfo(text).Exists)
				{
					continue;
				}
				foreach (VirtualDirectory directory in AbstractFilesystem.GetDirectories(text, "*", SearchOption.TopDirectoryOnly))
				{
					if (directory.Name == folderName || directory.Name == legacyFolderName)
					{
						info = DirectXmlLoader.ItemFromXmlFile<LanguageInfo>(directory, "LanguageInfo.xml", resolveCrossRefs: false);
						if (info.friendlyNameNative.NullOrEmpty() && directory.FileExists("FriendlyName.txt"))
						{
							info.friendlyNameNative = directory.ReadAllText("FriendlyName.txt");
						}
						if (info.friendlyNameNative.NullOrEmpty())
						{
							info.friendlyNameNative = folderName;
						}
						if (info.friendlyNameEnglish.NullOrEmpty())
						{
							info.friendlyNameEnglish = folderName;
						}
						return;
					}
				}
			}
		}
	}

	public void InitMetadata(VirtualDirectory directory)
	{
		infoIsRealMetadata = false;
		info = new LanguageInfo();
		string text = Regex.Replace(directory.Name, "(\\B[A-Z]+?(?=[A-Z][^A-Z])|\\B[A-Z]+?(?=[^A-Z]))", " $1");
		string friendlyNameEnglish = text;
		string friendlyNameNative = text;
		int num = text.FirstIndexOf((char c) => c == '(');
		int num2 = text.LastIndexOf(")");
		if (num >= 0 && num2 >= 0 && num2 > num)
		{
			friendlyNameEnglish = text.Substring(0, num - 1);
			friendlyNameNative = text.Substring(num + 1, num2 - num - 1);
		}
		info.friendlyNameEnglish = friendlyNameEnglish;
		info.friendlyNameNative = friendlyNameNative;
	}

	public void LoadData()
	{
		if (dataIsLoaded)
		{
			return;
		}
		dataIsLoaded = true;
		DeepProfiler.Start("Loading language data: " + folderName);
		try
		{
			tmpAlreadyLoadedFiles.Clear();
			foreach (Tuple<VirtualDirectory, ModContentPack, string> allDirectory in AllDirectories)
			{
				Tuple<VirtualDirectory, ModContentPack, string> localDirectory = allDirectory;
				if (!tmpAlreadyLoadedFiles.ContainsKey(localDirectory.Item2))
				{
					tmpAlreadyLoadedFiles[localDirectory.Item2] = new HashSet<string>();
				}
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					if (icon == BaseContent.BadTex)
					{
						VirtualFile file = localDirectory.Item1.GetFile("LangIcon.png");
						if (file.Exists)
						{
							icon = ModContentLoader<Texture2D>.LoadItem(file).contentItem;
						}
					}
				});
				VirtualDirectory directory = localDirectory.Item1.GetDirectory("CodeLinked");
				if (directory.Exists)
				{
					loadErrors.Add("Translations aren't called CodeLinked any more. Please rename to Keyed: " + directory);
				}
				else
				{
					directory = localDirectory.Item1.GetDirectory("Keyed");
				}
				if (directory.Exists)
				{
					List<VirtualFile> list = new List<VirtualFile>();
					foreach (VirtualFile file2 in directory.GetFiles("*.xml", SearchOption.AllDirectories))
					{
						if (TryRegisterFileIfNew(localDirectory, file2.FullPath))
						{
							list.Add(file2);
						}
					}
					List<string> list2 = (from x in list.AsParallel()
						select x.ReadAllText()).ToList();
					for (int num = 0; num < list2.Count; num++)
					{
						LoadFromFile_Keyed(list[num], list2[num]);
					}
				}
				VirtualDirectory directory2 = localDirectory.Item1.GetDirectory("DefLinked");
				if (directory2.Exists)
				{
					loadErrors.Add("Translations aren't called DefLinked any more. Please rename to DefInjected: " + directory2);
				}
				else
				{
					directory2 = localDirectory.Item1.GetDirectory("DefInjected");
				}
				if (directory2.Exists)
				{
					foreach (VirtualDirectory directory4 in directory2.GetDirectories("*", SearchOption.TopDirectoryOnly))
					{
						string name = directory4.Name;
						Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(name);
						if (typeInAnyAssembly == null && name.Length > 3)
						{
							typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(name.Substring(0, name.Length - 1));
						}
						if (typeInAnyAssembly == null)
						{
							loadErrors.Add("Error loading language from " + allDirectory?.ToString() + ": dir " + directory4.Name + " doesn't correspond to any def type. Skipping...");
							continue;
						}
						List<VirtualFile> list3 = new List<VirtualFile>();
						foreach (VirtualFile file3 in directory4.GetFiles("*.xml", SearchOption.AllDirectories))
						{
							if (TryRegisterFileIfNew(localDirectory, file3.FullPath))
							{
								list3.Add(file3);
							}
						}
						List<string> list4 = (from x in list3.AsParallel()
							select x.ReadAllText()).ToList();
						for (int num2 = 0; num2 < list4.Count; num2++)
						{
							LoadFromFile_DefInject(list3[num2], typeInAnyAssembly, list4[num2]);
						}
					}
				}
				EnsureAllDefTypesHaveDefInjectionPackage();
				VirtualDirectory directory3 = localDirectory.Item1.GetDirectory("Strings");
				if (directory3.Exists)
				{
					foreach (VirtualDirectory directory5 in directory3.GetDirectories("*", SearchOption.TopDirectoryOnly))
					{
						foreach (VirtualFile file4 in directory5.GetFiles("*.txt", SearchOption.AllDirectories))
						{
							if (TryRegisterFileIfNew(localDirectory, file4.FullPath))
							{
								LoadFromFile_Strings(file4, directory3);
							}
						}
					}
				}
				wordInfo.LoadFrom(localDirectory, this);
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception loading language data. Rethrowing. Exception: " + ex);
			throw;
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public bool TryRegisterFileIfNew(Tuple<VirtualDirectory, ModContentPack, string> dir, string filePath)
	{
		if (!filePath.StartsWith(dir.Item3))
		{
			Log.Error("Failed to get a relative path for a file: " + filePath + ", located in " + dir.Item3);
			return false;
		}
		string item = filePath.Substring(dir.Item3.Length);
		if (!tmpAlreadyLoadedFiles.ContainsKey(dir.Item2))
		{
			tmpAlreadyLoadedFiles[dir.Item2] = new HashSet<string>();
		}
		else if (tmpAlreadyLoadedFiles[dir.Item2].Contains(item))
		{
			return false;
		}
		tmpAlreadyLoadedFiles[dir.Item2].Add(item);
		return true;
	}

	private void LoadFromFile_Strings(VirtualFile file, VirtualDirectory stringsTopDir)
	{
		string text;
		try
		{
			text = file.ReadAllText();
		}
		catch (Exception ex)
		{
			loadErrors.Add("Exception loading from strings file " + file?.ToString() + ": " + ex);
			return;
		}
		string text2 = file.FullPath;
		if (stringsTopDir != null)
		{
			text2 = text2.Substring(stringsTopDir.FullPath.Length + 1);
		}
		text2 = text2.Substring(0, text2.Length - Path.GetExtension(text2).Length);
		text2 = text2.Replace('\\', '/');
		List<string> list = new List<string>();
		foreach (string item in GenText.LinesFromString(text))
		{
			list.Add(item);
		}
		if (stringFiles.TryGetValue(text2, out var value))
		{
			foreach (string item2 in list)
			{
				value.Add(item2);
			}
			return;
		}
		stringFiles.Add(text2, list);
	}

	private void LoadFromFile_Keyed(VirtualFile file, string preloadedFileContents)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
		try
		{
			foreach (DirectXmlLoaderSimple.XmlKeyValuePair item in DirectXmlLoaderSimple.ValuesFromXmlFile(preloadedFileContents))
			{
				if (dictionary.ContainsKey(item.key))
				{
					loadErrors.Add("Duplicate keyed translation key: " + item.key + " in language " + folderName);
					continue;
				}
				dictionary.Add(item.key, item.value);
				dictionary2.Add(item.key, item.lineNumber);
			}
		}
		catch (Exception ex)
		{
			loadErrors.Add("Exception loading from translation file " + file?.ToString() + ": " + ex);
			dictionary.Clear();
			dictionary2.Clear();
			anyKeyedReplacementsXmlParseError = true;
			lastKeyedReplacementsXmlParseErrorInFile = file.Name;
		}
		foreach (KeyValuePair<string, string> item2 in dictionary)
		{
			string text = item2.Value;
			KeyedReplacement keyedReplacement = new KeyedReplacement();
			if (text == "TODO")
			{
				keyedReplacement.isPlaceholder = true;
				text = "";
			}
			keyedReplacement.key = item2.Key;
			keyedReplacement.value = text;
			keyedReplacement.fileSource = file.Name;
			keyedReplacement.fileSourceLine = dictionary2[item2.Key];
			keyedReplacement.fileSourceFullPath = file.FullPath;
			keyedReplacements.SetOrAdd(item2.Key, keyedReplacement);
		}
	}

	public void LoadFromFile_DefInject(VirtualFile file, Type defType, string preloadedFileContents)
	{
		DefInjectionPackage defInjectionPackage = defInjections.Where((DefInjectionPackage di) => di.defType == defType).FirstOrDefault();
		if (defInjectionPackage == null)
		{
			defInjectionPackage = new DefInjectionPackage(defType);
			defInjections.Add(defInjectionPackage);
		}
		defInjectionPackage.AddDataFromFile(file, out var xmlParseError, preloadedFileContents);
		if (xmlParseError)
		{
			anyDefInjectionsXmlParseError = true;
			lastDefInjectionsXmlParseErrorInFile = file.Name;
		}
	}

	private void EnsureAllDefTypesHaveDefInjectionPackage()
	{
		foreach (Type defType in GenDefDatabase.AllDefTypesWithDatabases())
		{
			if (!defInjections.Any((DefInjectionPackage x) => x.defType == defType))
			{
				defInjections.Add(new DefInjectionPackage(defType));
			}
		}
	}

	public bool HaveTextForKey(string key, bool allowPlaceholders = false)
	{
		if (!dataIsLoaded)
		{
			LoadData();
		}
		if (key == null)
		{
			return false;
		}
		if (!keyedReplacements.TryGetValue(key, out var value))
		{
			return false;
		}
		if (!allowPlaceholders)
		{
			return !value.isPlaceholder;
		}
		return true;
	}

	public bool TryGetTextFromKey(string key, out TaggedString translated)
	{
		if (!dataIsLoaded)
		{
			LoadData();
		}
		if (key == null)
		{
			translated = key;
			return false;
		}
		if (!keyedReplacements.TryGetValue(key, out var value) || value.isPlaceholder)
		{
			translated = key;
			return false;
		}
		translated = value.value;
		return true;
	}

	public bool TryGetStringsFromFile(string fileName, out List<string> stringsList)
	{
		if (!dataIsLoaded)
		{
			LoadData();
		}
		if (!stringFiles.TryGetValue(fileName, out stringsList))
		{
			stringsList = null;
			return false;
		}
		return true;
	}

	public string GetKeySourceFileAndLine(string key)
	{
		if (!keyedReplacements.TryGetValue(key, out var value))
		{
			return "unknown";
		}
		return value.fileSource + ":" + value.fileSourceLine;
	}

	public Gender ResolveGender(string str, string fallback = null, Gender defaultGender = Gender.Male)
	{
		return wordInfo.ResolveGender(str, fallback, defaultGender);
	}

	public void InjectIntoData_BeforeImpliedDefs()
	{
		if (!dataIsLoaded)
		{
			LoadData();
		}
		foreach (DefInjectionPackage defInjection in defInjections)
		{
			try
			{
				defInjection.InjectIntoDefs(errorOnDefNotFound: false);
			}
			catch (Exception ex)
			{
				Log.Error("Critical error while injecting translations into defs: " + ex);
			}
		}
	}

	public void InjectIntoData_AfterImpliedDefs()
	{
		if (!dataIsLoaded)
		{
			LoadData();
		}
		int num = loadErrors.Count;
		foreach (DefInjectionPackage defInjection in defInjections)
		{
			try
			{
				defInjection.InjectIntoDefs(errorOnDefNotFound: true);
				num += defInjection.loadErrors.Count;
			}
			catch (Exception ex)
			{
				Log.Error("Critical error while injecting translations into defs: " + ex);
			}
		}
		if (num != 0)
		{
			anyError = true;
			Log.Warning("Translation data for language " + LanguageDatabase.activeLanguage.FriendlyNameEnglish + " has " + num + " errors. Generate translation report for more info.");
		}
	}

	public override string ToString()
	{
		return info.friendlyNameEnglish;
	}
}
