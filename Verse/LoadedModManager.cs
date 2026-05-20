using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace Verse;

public static class LoadedModManager
{
	private static List<ModContentPack> runningMods = new List<ModContentPack>();

	private static Dictionary<Type, Mod> runningModClasses = new Dictionary<Type, Mod>();

	private static List<Def> patchedDefs = new List<Def>();

	public static List<ModContentPack> RunningModsListForReading => runningMods;

	public static IEnumerable<ModContentPack> RunningMods => runningMods;

	public static List<Def> PatchedDefsForReading => patchedDefs;

	public static IEnumerable<Mod> ModHandles => runningModClasses.Values;

	public static void LoadAllActiveMods(bool hotReload = false)
	{
		DeepProfiler.Start("XmlInheritance.Clear()");
		try
		{
			XmlInheritance.Clear();
		}
		finally
		{
			DeepProfiler.End();
		}
		if (!hotReload)
		{
			DeepProfiler.Start("InitializeMods()");
			try
			{
				InitializeMods();
			}
			finally
			{
				DeepProfiler.End();
			}
		}
		DeepProfiler.Start("LoadModContent()");
		try
		{
			LoadModContent(hotReload);
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("CreateModClasses()");
		try
		{
			CreateModClasses();
		}
		finally
		{
			DeepProfiler.End();
		}
		List<LoadableXmlAsset> xmls = null;
		DeepProfiler.Start("LoadModXML()");
		try
		{
			xmls = LoadModXML(hotReload);
		}
		finally
		{
			DeepProfiler.End();
		}
		Dictionary<XmlNode, LoadableXmlAsset> assetlookup = new Dictionary<XmlNode, LoadableXmlAsset>();
		XmlDocument xmlDocument = null;
		DeepProfiler.Start("CombineIntoUnifiedXML()");
		try
		{
			xmlDocument = CombineIntoUnifiedXML(xmls, assetlookup);
		}
		finally
		{
			DeepProfiler.End();
		}
		if (!hotReload)
		{
			TKeySystem.Clear();
			DeepProfiler.Start("TKeySystem.Parse()");
			try
			{
				TKeySystem.Parse(xmlDocument);
			}
			finally
			{
				DeepProfiler.End();
			}
		}
		if (!hotReload)
		{
			DeepProfiler.Start("ErrorCheckPatches()");
			try
			{
				ErrorCheckPatches();
			}
			finally
			{
				DeepProfiler.End();
			}
		}
		DeepProfiler.Start("ApplyPatches()");
		try
		{
			ApplyPatches(xmlDocument, assetlookup);
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("ParseAndProcessXML()");
		try
		{
			ParseAndProcessXML(xmlDocument, assetlookup, hotReload);
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("ClearCachedPatches()");
		try
		{
			ClearCachedPatches();
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("XmlInheritance.Clear()");
		try
		{
			XmlInheritance.Clear();
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public static void InitializeMods()
	{
		int num = 0;
		foreach (ModMetaData item2 in ModsConfig.ActiveModsInLoadOrder.ToList())
		{
			DeepProfiler.Start("Initializing " + item2);
			try
			{
				if (!item2.RootDir.Exists)
				{
					ModsConfig.SetActive(item2.PackageId, active: false);
					Log.Warning("Failed to find active mod " + item2.Name + "(" + item2.PackageIdPlayerFacing + ") at " + item2.RootDir);
				}
				else
				{
					ModContentPack item = new ModContentPack(item2.RootDir, item2.PackageId, item2.PackageIdPlayerFacing, num, item2.Name, item2.Official);
					num++;
					runningMods.Add(item);
					GenTypes.ClearCache();
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error initializing mod: " + ex);
				ModsConfig.SetActive(item2.PackageId, active: false);
			}
			finally
			{
				DeepProfiler.End();
			}
		}
	}

	public static void LoadModContent(bool hotReload = false)
	{
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			DeepProfiler.Start("LoadModContent");
		});
		for (int num = 0; num < runningMods.Count; num++)
		{
			ModContentPack modContentPack = runningMods[num];
			DeepProfiler.Start("Loading " + modContentPack?.ToString() + " content");
			try
			{
				modContentPack.ReloadContent(hotReload);
			}
			catch (Exception ex)
			{
				Log.Error("Could not reload mod content for mod " + modContentPack.PackageIdPlayerFacing + ": " + ex);
			}
			finally
			{
				DeepProfiler.End();
			}
		}
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			DeepProfiler.End();
			for (int i = 0; i < runningMods.Count; i++)
			{
				ModContentPack modContentPack2 = runningMods[i];
				if (!modContentPack2.AnyContentLoaded())
				{
					Log.Error("Mod " + modContentPack2.Name + " did not load any content. Following load folders were used:\n" + modContentPack2.foldersToLoadDescendingOrder.ToLineList("  - "));
				}
			}
		});
	}

	public static void CreateModClasses()
	{
		foreach (Type type in typeof(Mod).InstantiableDescendantsAndSelf())
		{
			DeepProfiler.Start("Loading " + type?.ToString() + " mod class");
			try
			{
				if (!runningModClasses.ContainsKey(type))
				{
					ModContentPack modContentPack = runningMods.Where((ModContentPack modpack) => modpack.assemblies.loadedAssemblies.Contains(type.Assembly)).FirstOrDefault();
					runningModClasses[type] = (Mod)Activator.CreateInstance(type, modContentPack);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error while instantiating a mod of type " + type?.ToString() + ": " + ex);
			}
			finally
			{
				DeepProfiler.End();
			}
		}
	}

	public static List<LoadableXmlAsset> LoadModXML(bool hotReload = false)
	{
		List<LoadableXmlAsset> list = new List<LoadableXmlAsset>();
		for (int i = 0; i < runningMods.Count; i++)
		{
			ModContentPack modContentPack = runningMods[i];
			DeepProfiler.Start("Loading " + modContentPack);
			try
			{
				list.AddRange(modContentPack.LoadDefs(hotReload));
			}
			catch (Exception ex)
			{
				Log.Error("Could not load defs for mod " + modContentPack.PackageIdPlayerFacing + ": " + ex);
			}
			finally
			{
				DeepProfiler.End();
			}
		}
		return list;
	}

	public static void ErrorCheckPatches()
	{
		foreach (ModContentPack runningMod in runningMods)
		{
			foreach (PatchOperation patch in runningMod.Patches)
			{
				try
				{
					foreach (string item in patch.ConfigErrors())
					{
						Log.Error("Config error in " + runningMod.Name + " patch " + patch?.ToString() + ": " + item);
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception in ConfigErrors() of " + runningMod.Name + " patch " + patch?.ToString() + ": " + ex);
				}
			}
		}
	}

	public static void ApplyPatches(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
	{
		foreach (PatchOperation item in runningMods.SelectMany((ModContentPack rm) => rm.Patches))
		{
			try
			{
				item.Apply(xmlDoc);
			}
			catch (Exception ex)
			{
				Log.Error("Error in patch.Apply(): " + ex);
			}
		}
	}

	public static XmlDocument CombineIntoUnifiedXML(List<LoadableXmlAsset> xmls, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.AppendChild(xmlDocument.CreateElement("Defs"));
		foreach (LoadableXmlAsset xml in xmls)
		{
			if (xml.xmlDoc == null || xml.xmlDoc.DocumentElement == null)
			{
				Log.Error(string.Format("{0}: unknown parse failure", xml.fullFolderPath + "/" + xml.name));
				continue;
			}
			if (xml.xmlDoc.DocumentElement.Name != "Defs")
			{
				Log.Error(string.Format("{0}: root element named {1}; should be named Defs", xml.fullFolderPath + "/" + xml.name, xml.xmlDoc.DocumentElement.Name));
			}
			foreach (XmlNode childNode in xml.xmlDoc.DocumentElement.ChildNodes)
			{
				XmlNode xmlNode = xmlDocument.ImportNode(childNode, deep: true);
				assetlookup[xmlNode] = xml;
				xmlDocument.DocumentElement.AppendChild(xmlNode);
			}
		}
		return xmlDocument;
	}

	public static void ParseAndProcessXML(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup, bool hotReload = false)
	{
		XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
		List<XmlNode> list = new List<XmlNode>();
		foreach (object item3 in childNodes)
		{
			list.Add(item3 as XmlNode);
		}
		DeepProfiler.Start("Loading asset nodes " + list.Count);
		try
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].NodeType == XmlNodeType.Element)
				{
					LoadableXmlAsset value = null;
					DeepProfiler.Start("assetlookup.TryGetValue");
					try
					{
						assetlookup.TryGetValue(list[i], out value);
					}
					finally
					{
						DeepProfiler.End();
					}
					DeepProfiler.Start("XmlInheritance.TryRegister");
					try
					{
						XmlInheritance.TryRegister(list[i], value?.mod);
					}
					finally
					{
						DeepProfiler.End();
					}
				}
			}
		}
		finally
		{
			DeepProfiler.End();
		}
		DeepProfiler.Start("XmlInheritance.Resolve()");
		try
		{
			XmlInheritance.Resolve();
		}
		finally
		{
			DeepProfiler.End();
		}
		runningMods.FirstOrDefault();
		if (hotReload)
		{
			foreach (ModContentPack runningMod in runningMods)
			{
				runningMod.ClearDefs();
			}
		}
		patchedDefs.Clear();
		List<(Def, Def)> toHotReloadCopy = new List<(Def, Def)>();
		bool flag = !GenCommandLine.CommandLineArgPassed("legacy-xml-deserializer");
		DeepProfiler.Start("Loading defs for " + list.Count + " nodes");
		try
		{
			foreach (XmlNode item4 in list)
			{
				string text = item4.Attributes?["MayRequire"]?.Value.ToLower();
				if (text != null && !ModLister.AllModsActiveNoSuffix(text.Split(',')))
				{
					continue;
				}
				string[] array = item4.Attributes?["MayRequireAnyOf"]?.Value.ToLower().Split(',');
				if (!array.NullOrEmpty() && !ModLister.AnyModActiveNoSuffix(array))
				{
					continue;
				}
				LoadableXmlAsset loadableXmlAsset = assetlookup.TryGetValue(item4);
				Def def = (flag ? DirectXmlToObjectNew.DefFromNodeNew(item4, loadableXmlAsset) : DirectXmlLoader.DefFromNode(item4, loadableXmlAsset));
				if (def == null)
				{
					continue;
				}
				if (hotReload)
				{
					Def defSilentFail = GenDefDatabase.GetDefSilentFail(def.GetType(), def.defName);
					if (defSilentFail != null)
					{
						toHotReloadCopy.Add((def, defSilentFail));
						def = defSilentFail;
					}
				}
				ModContentPack modContentPack = loadableXmlAsset?.mod;
				if (modContentPack != null)
				{
					modContentPack.AddDef(def, loadableXmlAsset.name);
				}
				else
				{
					patchedDefs.Add(def);
				}
			}
		}
		finally
		{
			DeepProfiler.End();
		}
		if (toHotReloadCopy.Count == 0)
		{
			return;
		}
		LongEventHandler.ExecuteWhenFinished(delegate
		{
			Parallel.ForEach(toHotReloadCopy, delegate((Def, Def) toCopy)
			{
				Def item = toCopy.Item1;
				Def item2 = toCopy.Item2;
				ushort shortHash = item2.shortHash;
				ushort index = item2.index;
				ushort debugRandomId = item2.debugRandomId;
				ModContentPack modContentPack2 = item2.modContentPack;
				TreeNode_ThingCategory treeNode_ThingCategory = (item2 as ThingCategoryDef)?.treeNode;
				Gen.MemberwiseShallowCopy(item, item2);
				item2.shortHash = shortHash;
				item2.index = index;
				item2.debugRandomId = debugRandomId;
				item2.modContentPack = modContentPack2;
				if (treeNode_ThingCategory != null)
				{
					((ThingCategoryDef)item2).treeNode = treeNode_ThingCategory;
				}
				item.defName += "_HotReloadedThrowaway";
				item.ResolveDefNameHash();
				item.ClearCachedData();
			});
		});
	}

	public static void ClearCachedPatches()
	{
		foreach (ModContentPack runningMod in runningMods)
		{
			foreach (PatchOperation patch in runningMod.Patches)
			{
				try
				{
					patch.Complete(runningMod.Name);
				}
				catch (Exception ex)
				{
					Log.Error("Error in patch.Complete(): " + ex);
				}
			}
			runningMod.ClearPatchesCache();
		}
	}

	public static void ClearDestroy()
	{
		foreach (ModContentPack runningMod in runningMods)
		{
			try
			{
				runningMod.ClearDestroy();
			}
			catch (Exception ex)
			{
				Log.Error("Error in mod.ClearDestroy(): " + ex);
			}
		}
		runningMods.Clear();
		GenTypes.ClearCache();
	}

	public static T GetMod<T>() where T : Mod
	{
		return GetMod(typeof(T)) as T;
	}

	public static Mod GetMod(Type type)
	{
		if (runningModClasses.ContainsKey(type))
		{
			return runningModClasses[type];
		}
		return runningModClasses.Where((KeyValuePair<Type, Mod> kvp) => type.IsAssignableFrom(kvp.Key)).FirstOrDefault().Value;
	}

	private static string GetSettingsFilename(string modIdentifier, string modHandleName)
	{
		return Path.Combine(GenFilePaths.ConfigFolderPath, GenText.SanitizeFilename($"Mod_{modIdentifier}_{modHandleName}.xml"));
	}

	public static T ReadModSettings<T>(string modIdentifier, string modHandleName) where T : ModSettings, new()
	{
		string settingsFilename = GetSettingsFilename(modIdentifier, modHandleName);
		T target = null;
		try
		{
			if (File.Exists(settingsFilename))
			{
				Scribe.loader.InitLoading(settingsFilename);
				try
				{
					Scribe_Deep.Look(ref target, "ModSettings");
				}
				finally
				{
					Scribe.loader.FinalizeLoading();
				}
			}
		}
		catch (Exception ex)
		{
			Log.Warning($"Caught exception while loading mod settings data for {modIdentifier}. Generating fresh settings. The exception was: {ex.ToString()}");
			target = null;
		}
		if (target == null)
		{
			return new T();
		}
		return target;
	}

	public static void WriteModSettings(string modIdentifier, string modHandleName, ModSettings settings)
	{
		Scribe.saver.InitSaving(GetSettingsFilename(modIdentifier, modHandleName), "SettingsBlock");
		try
		{
			Scribe_Deep.Look(ref settings, "ModSettings");
		}
		finally
		{
			Scribe.saver.FinalizeSaving();
		}
	}
}
