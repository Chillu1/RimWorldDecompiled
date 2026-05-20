using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LudeonTK;
using UnityEngine;

namespace Verse;

public class DebugActionsMods
{
	[DebugAction("Mods", null, false, false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.Entry)]
	private static void LoadedFilesForMod()
	{
		List<DebugMenuOption> list = new List<DebugMenuOption>();
		foreach (ModContentPack item in LoadedModManager.RunningModsListForReading)
		{
			ModContentPack mod = item;
			list.Add(new DebugMenuOption(mod.Name, DebugMenuOptionMode.Action, delegate
			{
				ModMetaData metaData = ModLister.GetModWithIdentifier(mod.PackageId);
				if (metaData.loadFolders != null && metaData.loadFolders.DefinedVersions().Count != 0)
				{
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(from ver in metaData.loadFolders.DefinedVersions()
						select new DebugMenuOption(ver, DebugMenuOptionMode.Action, delegate
						{
							ShowTable((from f in metaData.loadFolders.FoldersForVersion(ver)
								select Path.Combine(mod.RootDir, f.folderName)).Reverse().ToList());
						})));
				}
				else
				{
					ShowTable(null);
				}
			}));
			void ShowTable(List<string> loadFolders)
			{
				List<Pair<string, string>> list2 = new List<Pair<string, string>>();
				list2.AddRange(from f in DirectXmlLoader.XmlAssetsInModFolder(mod, "Defs/", loadFolders)
					select new Pair<string, string>(f.FullFilePath, "-"));
				list2.AddRange(from f in DirectXmlLoader.XmlAssetsInModFolder(mod, "Patches/", loadFolders)
					select new Pair<string, string>(f.FullFilePath, "-"));
				list2.AddRange(from f in ModContentPack.GetAllFilesForMod(mod, GenFilePaths.ContentPath<Texture2D>(), ModContentLoader<Texture2D>.IsAcceptableExtension, loadFolders)
					select new Pair<string, string>(f.Value.FullName, f.Key));
				list2.AddRange(from f in ModContentPack.GetAllFilesForMod(mod, GenFilePaths.ContentPath<AudioClip>(), ModContentLoader<AudioClip>.IsAcceptableExtension, loadFolders)
					select new Pair<string, string>(f.Value.FullName, f.Key));
				list2.AddRange(from f in ModContentPack.GetAllFilesForMod(mod, GenFilePaths.ContentPath<string>(), ModContentLoader<string>.IsAcceptableExtension, loadFolders)
					select new Pair<string, string>(f.Value.FullName, f.Key));
				list2.AddRange(from f in ModContentPack.GetAllFilesForModPreserveOrder(mod, "Assemblies/", (string e) => e.ToLower() == ".dll", loadFolders)
					select new Pair<string, string>(f.Item2.FullName, f.Item1));
				DebugTables.MakeTablesDialog(list2, new List<TableDataGetter<Pair<string, string>>>
				{
					new TableDataGetter<Pair<string, string>>("full path", (Pair<string, string> f) => f.First),
					new TableDataGetter<Pair<string, string>>("internal path", (Pair<string, string> f) => f.Second)
				}.ToArray());
			}
		}
		Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
	}
}
