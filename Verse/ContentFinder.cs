using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class ContentFinder<T> where T : class
	{
		public static T Get(string itemPath, bool reportFailure = true)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to get a resource \"" + itemPath + "\" from a different thread. All resources must be loaded in the main thread.");
				return null;
			}
			T val = null;
			List<ModContentPack> runningModsListForReading = LoadedModManager.RunningModsListForReading;
			for (int num = runningModsListForReading.Count - 1; num >= 0; num--)
			{
				val = runningModsListForReading[num].GetContentHolder<T>().Get(itemPath);
				if (val != null)
				{
					return val;
				}
			}
			if (typeof(T) == typeof(Texture2D))
			{
				val = (T)(object)Resources.Load<Texture2D>(GenFilePaths.ContentPath<Texture2D>() + itemPath);
			}
			if (typeof(T) == typeof(AudioClip))
			{
				val = (T)(object)Resources.Load<AudioClip>(GenFilePaths.ContentPath<AudioClip>() + itemPath);
			}
			if (val != null)
			{
				return val;
			}
			for (int num2 = runningModsListForReading.Count - 1; num2 >= 0; num2--)
			{
				for (int i = 0; i < runningModsListForReading[num2].assetBundles.loadedAssetBundles.Count; i++)
				{
					AssetBundle assetBundle = runningModsListForReading[num2].assetBundles.loadedAssetBundles[i];
					string path = Path.Combine("Assets", "Data");
					path = Path.Combine(path, runningModsListForReading[num2].FolderName);
					if (typeof(T) == typeof(Texture2D))
					{
						string str = Path.Combine(Path.Combine(path, GenFilePaths.ContentPath<Texture2D>()), itemPath);
						for (int j = 0; j < ModAssetBundlesHandler.TextureExtensions.Length; j++)
						{
							val = (T)(object)assetBundle.LoadAsset<Texture2D>(str + ModAssetBundlesHandler.TextureExtensions[j]);
							if (val != null)
							{
								return val;
							}
						}
					}
					if (!(typeof(T) == typeof(AudioClip)))
					{
						continue;
					}
					string str2 = Path.Combine(Path.Combine(path, GenFilePaths.ContentPath<AudioClip>()), itemPath);
					for (int k = 0; k < ModAssetBundlesHandler.AudioClipExtensions.Length; k++)
					{
						val = (T)(object)assetBundle.LoadAsset<AudioClip>(str2 + ModAssetBundlesHandler.AudioClipExtensions[k]);
						if (val != null)
						{
							return val;
						}
					}
				}
			}
			if (reportFailure)
			{
				Log.Error(string.Concat("Could not load ", typeof(T), " at ", itemPath, " in any active mod or in base resources."));
			}
			return null;
		}

		public static IEnumerable<T> GetAllInFolder(string folderPath)
		{
			if (!UnityData.IsInMainThread)
			{
				Log.Error("Tried to get all resources in a folder \"" + folderPath + "\" from a different thread. All resources must be loaded in the main thread.");
				yield break;
			}
			foreach (ModContentPack runningMod in LoadedModManager.RunningMods)
			{
				foreach (T item in runningMod.GetContentHolder<T>().GetAllUnderPath(folderPath))
				{
					yield return item;
				}
			}
			T[] array = null;
			if (typeof(T) == typeof(Texture2D))
			{
				array = (T[])(object)Resources.LoadAll<Texture2D>(GenFilePaths.ContentPath<Texture2D>() + folderPath);
			}
			if (typeof(T) == typeof(AudioClip))
			{
				array = (T[])(object)Resources.LoadAll<AudioClip>(GenFilePaths.ContentPath<AudioClip>() + folderPath);
			}
			if (array != null)
			{
				T[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					yield return array2[j];
				}
			}
			List<ModContentPack> mods = LoadedModManager.RunningModsListForReading;
			for (int j = mods.Count - 1; j >= 0; j--)
			{
				for (int i = 0; i < mods[j].assetBundles.loadedAssetBundles.Count; i++)
				{
					AssetBundle assetBundle = mods[j].assetBundles.loadedAssetBundles[i];
					string dirForBundle2 = Path.Combine("Assets", "Data");
					dirForBundle2 = Path.Combine(dirForBundle2, mods[j].FolderName);
					if (typeof(T) == typeof(Texture2D))
					{
						string fullPath = Path.Combine(Path.Combine(dirForBundle2, GenFilePaths.ContentPath<Texture2D>()).Replace('\\', '/'), folderPath).ToLower();
						IEnumerable<string> enumerable = from p in mods[j].AllAssetNamesInBundle(i)
							where p.StartsWith(fullPath)
							select p;
						foreach (string item2 in enumerable)
						{
							if (ModAssetBundlesHandler.TextureExtensions.Contains(Path.GetExtension(item2)))
							{
								yield return (T)(object)assetBundle.LoadAsset<Texture2D>(item2);
							}
						}
					}
					if (!(typeof(T) == typeof(AudioClip)))
					{
						continue;
					}
					string fullPath2 = Path.Combine(Path.Combine(dirForBundle2, GenFilePaths.ContentPath<AudioClip>()).Replace('\\', '/'), folderPath).ToLower();
					IEnumerable<string> enumerable2 = from p in mods[j].AllAssetNamesInBundle(i)
						where p.StartsWith(fullPath2)
						select p;
					foreach (string item3 in enumerable2)
					{
						if (ModAssetBundlesHandler.AudioClipExtensions.Contains(Path.GetExtension(item3)))
						{
							yield return (T)(object)assetBundle.LoadAsset<AudioClip>(item3);
						}
					}
				}
			}
		}
	}
}
