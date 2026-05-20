using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using KTrie;
using RimWorld;
using UnityEngine;

namespace Verse;

public class ModContentPack
{
	private DirectoryInfo rootDirInt;

	public int loadOrder;

	private string nameInt;

	private string packageIdInt;

	private string packageIdPlayerFacingInt;

	private bool official;

	private ModContentHolder<AudioClip> audioClips;

	private ModContentHolder<Texture2D> textures;

	private ModContentHolder<string> strings;

	public ModAssetBundlesHandler assetBundles;

	public ModAssemblyHandler assemblies;

	private List<PatchOperation> patches;

	private List<Def> defs = new List<Def>();

	private List<List<string>> allAssetNamesInBundleCached;

	private List<StringTrieSet> allAssetNamesInBundleCachedTrie;

	public List<string> foldersToLoadDescendingOrder;

	private bool loadedAnyPatches;

	public const string LudeonPackageIdAuthor = "ludeon";

	public const string CoreModPackageId = "ludeon.rimworld";

	public const string RoyaltyModPackageId = "ludeon.rimworld.royalty";

	public const string IdeologyModPackageId = "ludeon.rimworld.ideology";

	public const string BiotechModPackageId = "ludeon.rimworld.biotech";

	public const string AnomalyModPackageId = "ludeon.rimworld.anomaly";

	public const string OdysseyModPackageId = "ludeon.rimworld.odyssey";

	public static readonly string[] ProductPackageIDs = new string[6] { "ludeon.rimworld", "ludeon.rimworld.royalty", "ludeon.rimworld.ideology", "ludeon.rimworld.biotech", "ludeon.rimworld.anomaly", "ludeon.rimworld.odyssey" };

	public static readonly string CommonFolderName = "Common";

	public string RootDir => rootDirInt.FullName;

	public string PackageId => packageIdInt;

	public string PackageIdPlayerFacing => packageIdPlayerFacingInt;

	public uint SteamAppId => ModMetaData?.SteamAppId ?? 0;

	public ModMetaData ModMetaData => ModLister.GetModWithIdentifier(PackageId);

	public string FolderName => rootDirInt.Name;

	public string Name => nameInt;

	public int OverwritePriority
	{
		get
		{
			if (!IsCoreMod)
			{
				return 1;
			}
			return 0;
		}
	}

	public bool IsCoreMod => PackageId == "ludeon.rimworld";

	public bool IsOfficialMod => official;

	public IEnumerable<Def> AllDefs => defs;

	public IEnumerable<PatchOperation> Patches
	{
		get
		{
			if (patches == null)
			{
				LoadPatches();
			}
			return patches;
		}
	}

	public List<string> AllAssetNamesInBundle(int index)
	{
		if (allAssetNamesInBundleCached == null)
		{
			allAssetNamesInBundleCached = new List<List<string>>();
			foreach (AssetBundle loadedAssetBundle in assetBundles.loadedAssetBundles)
			{
				allAssetNamesInBundleCached.Add(new List<string>(loadedAssetBundle.GetAllAssetNames()));
			}
		}
		return allAssetNamesInBundleCached[index];
	}

	public StringTrieSet AllAssetNamesInBundleTrie(int index)
	{
		if (allAssetNamesInBundleCachedTrie == null)
		{
			allAssetNamesInBundleCachedTrie = new List<StringTrieSet>();
			foreach (AssetBundle loadedAssetBundle in assetBundles.loadedAssetBundles)
			{
				StringTrieSet stringTrieSet = new StringTrieSet();
				string[] allAssetNames = loadedAssetBundle.GetAllAssetNames();
				foreach (string item in allAssetNames)
				{
					stringTrieSet.Add(item);
				}
				allAssetNamesInBundleCachedTrie.Add(stringTrieSet);
			}
		}
		return allAssetNamesInBundleCachedTrie[index];
	}

	public ModContentPack(DirectoryInfo directory, string packageId, string packageIdPlayerFacing, int loadOrder, string name, bool official)
	{
		rootDirInt = directory;
		this.loadOrder = loadOrder;
		nameInt = name;
		this.official = official;
		packageIdInt = packageId.ToLower();
		packageIdPlayerFacingInt = packageIdPlayerFacing;
		audioClips = new ModContentHolder<AudioClip>(this);
		textures = new ModContentHolder<Texture2D>(this);
		strings = new ModContentHolder<string>(this);
		assetBundles = new ModAssetBundlesHandler(this);
		assemblies = new ModAssemblyHandler(this);
		InitLoadFolders();
	}

	public void ClearDestroy()
	{
		audioClips.ClearDestroy();
		textures.ClearDestroy();
		assetBundles.ClearDestroy();
		allAssetNamesInBundleCached = null;
		allAssetNamesInBundleCachedTrie = null;
	}

	public ModContentHolder<T> GetContentHolder<T>() where T : class
	{
		if (typeof(T) == typeof(Texture2D))
		{
			return (ModContentHolder<T>)(object)textures;
		}
		if (typeof(T) == typeof(AudioClip))
		{
			return (ModContentHolder<T>)(object)audioClips;
		}
		if (typeof(T) == typeof(string))
		{
			return (ModContentHolder<T>)(object)strings;
		}
		Log.Error("Mod lacks manager for asset type " + strings);
		return null;
	}

	private void ReloadContentInt(bool hotReload = false)
	{
		if (hotReload)
		{
			return;
		}
		DeepProfiler.Start("Reload audio clips");
		try
		{
			audioClips.ReloadAll(hotReload);
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("Reload textures");
		try
		{
			textures.ReloadAll(hotReload);
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("Reload strings");
		try
		{
			strings.ReloadAll(hotReload);
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("Reload asset bundles");
		try
		{
			assetBundles.ReloadAll(hotReload);
			allAssetNamesInBundleCached = null;
			allAssetNamesInBundleCachedTrie = null;
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public void ReloadContent(bool hotReload = false)
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			ReloadContentInt(hotReload);
		});
		if (!hotReload)
		{
			assemblies.ReloadAll();
		}
	}

	public IEnumerable<LoadableXmlAsset> LoadDefs(bool hotReload = false)
	{
		if (!hotReload && defs.Count != 0)
		{
			Log.ErrorOnce("LoadDefs called with already existing def packages", 39029405);
		}
		DeepProfiler.Start("Load defs via DirectXmlLoader");
		List<LoadableXmlAsset> list = DirectXmlLoader.XmlAssetsInModFolder(this, "Defs/").ToList();
		DeepProfiler.End();
		DeepProfiler.Start("Parse loaded defs");
		foreach (LoadableXmlAsset item in list)
		{
			yield return item;
		}
		DeepProfiler.End();
	}

	private void InitLoadFolders()
	{
		foldersToLoadDescendingOrder = new List<string>();
		ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(PackageId);
		if (modWithIdentifier?.loadFolders != null && modWithIdentifier.loadFolders.DefinedVersions().Count > 0)
		{
			List<LoadFolder> list = modWithIdentifier.LoadFoldersForVersion(VersionControl.CurrentVersionString);
			if (list != null && list.Count > 0)
			{
				AddFolders(list);
				return;
			}
			string text = (from x in modWithIdentifier.loadFolders.DefinedVersions().Where(delegate(string x)
				{
					if (x == "default" || x.NullOrEmpty() || !x.Contains("."))
					{
						return false;
					}
					try
					{
						return VersionControl.VersionFromString(x) <= VersionControl.CurrentVersion;
					}
					catch
					{
						return false;
					}
				})
				orderby x descending
				select x).FirstOrDefault();
			if (text != null)
			{
				List<LoadFolder> list2 = modWithIdentifier.LoadFoldersForVersion(text);
				if (list2 != null)
				{
					AddFolders(list2);
					return;
				}
			}
			List<LoadFolder> list3 = modWithIdentifier.LoadFoldersForVersion("default");
			if (list3 != null)
			{
				AddFolders(list3);
				return;
			}
		}
		if (foldersToLoadDescendingOrder.Count != 0)
		{
			return;
		}
		string text2 = Path.Combine(RootDir, VersionControl.CurrentVersionStringWithoutBuild);
		if (Directory.Exists(text2))
		{
			foldersToLoadDescendingOrder.Add(text2);
		}
		else
		{
			Version version = new Version(0, 0);
			List<Version> list4 = new List<Version>();
			DirectoryInfo[] directories = rootDirInt.GetDirectories();
			for (int num = 0; num < directories.Length; num++)
			{
				if (VersionControl.TryParseVersionString(directories[num].Name, out var version2))
				{
					list4.Add(version2);
				}
			}
			list4.Sort();
			foreach (Version item in list4)
			{
				if ((item > version || version > VersionControl.CurrentVersion) && (item <= VersionControl.CurrentVersion || version.Major == 0))
				{
					version = item;
				}
			}
			if (version.Major > 0)
			{
				foldersToLoadDescendingOrder.Add(Path.Combine(RootDir, version.ToString()));
			}
		}
		string text3 = Path.Combine(RootDir, CommonFolderName);
		if (Directory.Exists(text3))
		{
			foldersToLoadDescendingOrder.Add(text3);
		}
		foldersToLoadDescendingOrder.Add(RootDir);
		void AddFolders(List<LoadFolder> folders)
		{
			for (int num2 = folders.Count - 1; num2 >= 0; num2--)
			{
				if (folders[num2].ShouldLoad)
				{
					foldersToLoadDescendingOrder.Add(Path.Combine(RootDir, folders[num2].folderName));
				}
			}
		}
	}

	private void LoadPatches()
	{
		DeepProfiler.Start("Loading all patches");
		patches = new List<PatchOperation>();
		loadedAnyPatches = false;
		List<LoadableXmlAsset> list = DirectXmlLoader.XmlAssetsInModFolder(this, "Patches/").ToList();
		for (int i = 0; i < list.Count; i++)
		{
			XmlElement documentElement = list[i].xmlDoc.DocumentElement;
			if (documentElement.Name != "Patch")
			{
				Log.Error($"Unexpected document element in patch XML; got {documentElement.Name}, expected 'Patch'");
				continue;
			}
			foreach (XmlNode childNode in documentElement.ChildNodes)
			{
				if (childNode.NodeType == XmlNodeType.Element)
				{
					if (childNode.Name != "Operation")
					{
						Log.Error($"Unexpected element in patch XML; got {childNode.Name}, expected 'Operation'");
						continue;
					}
					PatchOperation patchOperation = DirectXmlToObject.ObjectFromXml<PatchOperation>(childNode, doPostLoad: false);
					patchOperation.sourceFile = list[i].FullFilePath;
					patches.Add(patchOperation);
					loadedAnyPatches = true;
				}
			}
		}
		DeepProfiler.End();
	}

	public static Dictionary<string, FileInfo> GetAllFilesForMod(ModContentPack mod, string contentPath, Func<string, bool> validateExtension = null, List<string> foldersToLoadDebug = null)
	{
		List<string> list = foldersToLoadDebug ?? mod.foldersToLoadDescendingOrder;
		Dictionary<string, FileInfo> dictionary = new Dictionary<string, FileInfo>();
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i];
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(text, contentPath));
			if (!directoryInfo.Exists)
			{
				continue;
			}
			FileInfo[] files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
			foreach (FileInfo fileInfo in files)
			{
				if (validateExtension == null || validateExtension(fileInfo.Extension))
				{
					string key = fileInfo.FullName.Substring(text.Length + 1);
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, fileInfo);
					}
				}
			}
		}
		return dictionary;
	}

	public static List<Tuple<string, FileInfo>> GetAllFilesForModPreserveOrder(ModContentPack mod, string contentPath, Func<string, bool> validateExtension = null, List<string> foldersToLoadDebug = null)
	{
		List<string> list = foldersToLoadDebug ?? mod.foldersToLoadDescendingOrder;
		List<Tuple<string, FileInfo>> list2 = new List<Tuple<string, FileInfo>>();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			string text = list[num];
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(text, contentPath));
			if (directoryInfo.Exists)
			{
				FileInfo[] files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
				Array.Sort(files, (FileInfo a, FileInfo b) => a.Name.CompareTo(b.Name));
				foreach (FileInfo fileInfo in files)
				{
					if (validateExtension == null || validateExtension(fileInfo.Extension))
					{
						string item = fileInfo.FullName.Substring(text.Length + 1);
						list2.Add(new Tuple<string, FileInfo>(item, fileInfo));
					}
				}
			}
		}
		HashSet<string> hashSet = new HashSet<string>();
		for (int num3 = list2.Count - 1; num3 >= 0; num3--)
		{
			Tuple<string, FileInfo> tuple = list2[num3];
			if (!hashSet.Contains(tuple.Item1))
			{
				hashSet.Add(tuple.Item1);
			}
			else
			{
				list2.RemoveAt(num3);
			}
		}
		return list2;
	}

	public bool AnyContentLoaded()
	{
		if (AnyNonTranslationContentLoaded())
		{
			return true;
		}
		if (AnyTranslationsLoaded())
		{
			return true;
		}
		return false;
	}

	public bool AnyNonTranslationContentLoaded()
	{
		if (textures.contentList != null && textures.contentList.Count != 0)
		{
			return true;
		}
		if (audioClips.contentList != null && audioClips.contentList.Count != 0)
		{
			return true;
		}
		if (strings.contentList != null && strings.contentList.Count != 0)
		{
			return true;
		}
		if (!assemblies.loadedAssemblies.NullOrEmpty())
		{
			return true;
		}
		if (!assetBundles.loadedAssetBundles.NullOrEmpty())
		{
			return true;
		}
		if (loadedAnyPatches)
		{
			return true;
		}
		if (AllDefs.Any())
		{
			return true;
		}
		return false;
	}

	public bool AnyTranslationsLoaded()
	{
		foreach (string item in foldersToLoadDescendingOrder)
		{
			string path = Path.Combine(item, "Languages");
			if (Directory.Exists(path) && Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any())
			{
				return true;
			}
		}
		return false;
	}

	public void ClearPatchesCache()
	{
		patches = null;
	}

	public void ClearDefs()
	{
		defs.Clear();
	}

	public void AddDef(Def def, string source = "Unknown")
	{
		def.modContentPack = this;
		def.fileName = source;
		defs.Add(def);
	}

	public override string ToString()
	{
		return PackageIdPlayerFacing;
	}
}
