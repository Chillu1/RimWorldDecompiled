using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Verse
{
	public static class LoadedModManager
	{
		private static List<ModContentPack> runningMods = new List<ModContentPack>();

		private static Dictionary<Type, Mod> runningModClasses = new Dictionary<Type, Mod>();

		private static List<Def> patchedDefs = new List<Def>();

		public static List<ModContentPack> RunningModsListForReading => runningMods;

		public static IEnumerable<ModContentPack> RunningMods => runningMods;

		public static List<Def> PatchedDefsForReading => patchedDefs;

		public static IEnumerable<Mod> ModHandles => runningModClasses.Values;

		public static void LoadAllActiveMods()
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
			DeepProfiler.Start("InitializeMods()");
			try
			{
				InitializeMods();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("LoadModContent()");
			try
			{
				LoadModContent();
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
				xmls = LoadModXML();
			}
			finally
			{
				DeepProfiler.End();
			}
			Dictionary<XmlNode, LoadableXmlAsset> assetlookup = new Dictionary<XmlNode, LoadableXmlAsset>();
			XmlDocument xmlDoc = null;
			DeepProfiler.Start("CombineIntoUnifiedXML()");
			try
			{
				xmlDoc = CombineIntoUnifiedXML(xmls, assetlookup);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("ApplyPatches()");
			try
			{
				ApplyPatches(xmlDoc, assetlookup);
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("ParseAndProcessXML()");
			try
			{
				ParseAndProcessXML(xmlDoc, assetlookup);
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
						ModContentPack item = new ModContentPack(item2.RootDir, item2.PackageId, item2.PackageIdPlayerFacing, num, item2.Name);
						num++;
						runningMods.Add(item);
					}
				}
				catch (Exception arg)
				{
					Log.Error("Error initializing mod: " + arg);
					ModsConfig.SetActive(item2.PackageId, active: false);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}

		public static void LoadModContent()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				DeepProfiler.Start("LoadModContent");
			});
			for (int i = 0; i < runningMods.Count; i++)
			{
				ModContentPack modContentPack = runningMods[i];
				DeepProfiler.Start("Loading " + modContentPack + " content");
				try
				{
					modContentPack.ReloadContent();
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
				for (int j = 0; j < runningMods.Count; j++)
				{
					ModContentPack modContentPack2 = runningMods[j];
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
				DeepProfiler.Start("Loading " + type + " mod class");
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
					Log.Error("Error while instantiating a mod of type " + type + ": " + ex);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
		}

		public static List<LoadableXmlAsset> LoadModXML()
		{
			List<LoadableXmlAsset> list = new List<LoadableXmlAsset>();
			for (int i = 0; i < runningMods.Count; i++)
			{
				ModContentPack modContentPack = runningMods[i];
				DeepProfiler.Start("Loading " + modContentPack);
				try
				{
					list.AddRange(modContentPack.LoadDefs());
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

		public static void ApplyPatches(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
		{
			foreach (PatchOperation item in runningMods.SelectMany((ModContentPack rm) => rm.Patches))
			{
				try
				{
					item.Apply(xmlDoc);
				}
				catch (Exception arg)
				{
					Log.Error("Error in patch.Apply(): " + arg);
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
				}
				else
				{
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
			}
			return xmlDocument;
		}

		public static void ParseAndProcessXML(XmlDocument xmlDoc, Dictionary<XmlNode, LoadableXmlAsset> assetlookup)
		{
			XmlNodeList childNodes = xmlDoc.DocumentElement.ChildNodes;
			List<XmlNode> list = new List<XmlNode>();
			foreach (object item in childNodes)
			{
				list.Add(item as XmlNode);
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
			DeepProfiler.Start("Loading defs for " + list.Count + " nodes");
			try
			{
				foreach (XmlNode item2 in list)
				{
					LoadableXmlAsset loadableXmlAsset = assetlookup.TryGetValue(item2);
					Def def = DirectXmlLoader.DefFromNode(item2, loadableXmlAsset);
					if (def != null)
					{
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
			}
			finally
			{
				DeepProfiler.End();
			}
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
					catch (Exception arg)
					{
						Log.Error("Error in patch.Complete(): " + arg);
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
				catch (Exception arg)
				{
					Log.Error("Error in mod.ClearDestroy(): " + arg);
				}
			}
			runningMods.Clear();
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
}
