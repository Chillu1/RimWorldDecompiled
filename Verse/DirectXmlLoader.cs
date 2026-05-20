using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using RimWorld.IO;
using UnityEngine;

namespace Verse;

public static class DirectXmlLoader
{
	private static readonly LoadableXmlAsset[] EmptyXmlAssetsArray = Array.Empty<LoadableXmlAsset>();

	public static LoadableXmlAsset[] XmlAssetsInModFolder(ModContentPack mod, string folderPath, List<string> foldersToLoadDebug = null)
	{
		List<string> list = foldersToLoadDebug ?? mod.foldersToLoadDescendingOrder;
		Dictionary<string, FileInfo> dictionary = new Dictionary<string, FileInfo>();
		for (int i = 0; i < list.Count; i++)
		{
			string text = list[i];
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(text, folderPath));
			if (directoryInfo.Exists)
			{
				FileInfo[] files = directoryInfo.GetFiles("*.xml", SearchOption.AllDirectories);
				foreach (FileInfo fileInfo in files)
				{
					string key = fileInfo.FullName.Substring(text.Length + 1);
					dictionary.TryAdd(key, fileInfo);
				}
			}
		}
		if (dictionary.Count == 0)
		{
			return EmptyXmlAssetsArray;
		}
		List<FileInfo> list2 = dictionary.Values.ToList();
		LoadableXmlAsset[] assets = new LoadableXmlAsset[list2.Count];
		ConcurrentBag<KeyValuePair<int, FileInfo>> toLoad = new ConcurrentBag<KeyValuePair<int, FileInfo>>();
		for (int k = 0; k < list2.Count; k++)
		{
			int key2 = k;
			FileInfo value = list2[k];
			toLoad.Add(new KeyValuePair<int, FileInfo>(key2, value));
		}
		Thread[] array = new Thread[2];
		for (int l = 0; l < array.Length; l++)
		{
			array[l] = new Thread((ThreadStart)delegate
			{
				KeyValuePair<int, FileInfo> result2;
				while (toLoad.TryTake(out result2))
				{
					int key4 = result2.Key;
					FileInfo value3 = result2.Value;
					assets[key4] = new LoadableXmlAsset(value3, mod);
				}
			})
			{
				Name = $"DirectXmlLoader Thread {l + 1} of {2}"
			};
			array[l].Start();
		}
		KeyValuePair<int, FileInfo> result;
		while (toLoad.TryTake(out result))
		{
			int key3 = result.Key;
			FileInfo value2 = result.Value;
			assets[key3] = new LoadableXmlAsset(value2, mod);
		}
		Thread[] array2 = array;
		for (int num = 0; num < array2.Length; num++)
		{
			array2[num].Join();
		}
		return assets;
	}

	public static IEnumerable<T> LoadXmlDataInResourcesFolder<T>(string folderPath) where T : new()
	{
		XmlInheritance.Clear();
		DeepProfiler.Start("Resources.LoadAll<TextAsset>");
		TextAsset[] source = Resources.LoadAll<TextAsset>(folderPath);
		DeepProfiler.End();
		DeepProfiler.Start("Load XML");
		List<LoadableXmlAsset> assets = (from x in source.Select((TextAsset x) => new { x.name, x.text }).ToList().AsParallel()
			select new LoadableXmlAsset(x.name, x.text)).ToList();
		DeepProfiler.End();
		DeepProfiler.Start("Resolve inheritance");
		foreach (LoadableXmlAsset item in assets)
		{
			XmlInheritance.TryRegisterAllFrom(item, null);
		}
		XmlInheritance.Resolve();
		DeepProfiler.End();
		DeepProfiler.Start("Read game items from XML");
		for (int i = 0; i < assets.Count; i++)
		{
			foreach (T item2 in AllGameItemsFromAsset<T>(assets[i]))
			{
				yield return item2;
			}
		}
		DeepProfiler.End();
		XmlInheritance.Clear();
	}

	public static T ItemFromXmlFile<T>(string filePath, bool resolveCrossRefs = true) where T : new()
	{
		if (!new FileInfo(filePath).Exists)
		{
			return new T();
		}
		return ItemFromXmlString<T>(File.ReadAllText(filePath), filePath, resolveCrossRefs);
	}

	public static T ItemFromXmlFile<T>(VirtualDirectory directory, string filePath, bool resolveCrossRefs = true) where T : new()
	{
		if (!directory.FileExists(filePath))
		{
			return new T();
		}
		return ItemFromXmlString<T>(directory.ReadAllText(filePath), directory.FullPath + "/" + filePath, resolveCrossRefs);
	}

	public static T ItemFromXmlString<T>(string xmlContent, string filePath, bool resolveCrossRefs = true) where T : new()
	{
		if (resolveCrossRefs && DirectXmlCrossRefLoader.LoadingInProgress)
		{
			Log.Error("Cannot call ItemFromXmlString with resolveCrossRefs=true while loading is already in progress (forgot to resolve or clear cross refs from previous loading?).");
		}
		try
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlContent);
			T result = DirectXmlToObject.ObjectFromXml<T>(xmlDocument.DocumentElement, doPostLoad: false);
			if (resolveCrossRefs)
			{
				try
				{
					DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
				}
				finally
				{
					DirectXmlCrossRefLoader.Clear();
				}
			}
			return result;
		}
		catch (Exception ex)
		{
			Log.Error("Exception loading file at " + filePath + ". Loading defaults instead. Exception was: " + ex.ToString());
			return new T();
		}
	}

	public static Def DefFromNode(XmlNode node, LoadableXmlAsset loadingAsset)
	{
		if (node.NodeType != XmlNodeType.Element)
		{
			return null;
		}
		XmlAttribute xmlAttribute = node.Attributes["Abstract"];
		if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
		{
			return null;
		}
		XmlNode resolvedNodeFor = XmlInheritance.GetResolvedNodeFor(node);
		string text = node.Name;
		XmlAttribute xmlAttribute2 = resolvedNodeFor.Attributes["Class"];
		if (xmlAttribute2 != null)
		{
			text = xmlAttribute2.Value;
		}
		Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(node.Name);
		if (typeInAnyAssembly == null || !GenTypes.IsDef(typeInAnyAssembly))
		{
			Log.ErrorOnce("Type " + text + " is not a Def type or could not be found, in file " + ((loadingAsset != null) ? loadingAsset.name : "(unknown)") + ". Context: " + node.OuterXml, text.GetHashCode());
			return null;
		}
		Func<XmlNode, bool, object> objectFromXmlMethod = DirectXmlToObject.GetObjectFromXmlMethod(typeInAnyAssembly);
		Def def = null;
		try
		{
			def = (Def)objectFromXmlMethod(node, arg2: true);
			def.ResolveDefNameHash();
		}
		catch (Exception ex)
		{
			Log.Error("Exception loading def from file " + ((loadingAsset != null) ? loadingAsset.name : "(unknown)") + ": " + ex);
		}
		return def;
	}

	public static IEnumerable<T> AllGameItemsFromAsset<T>(LoadableXmlAsset asset) where T : new()
	{
		if (asset.xmlDoc == null)
		{
			yield break;
		}
		XmlNodeList xmlNodeList = asset.xmlDoc.DocumentElement.SelectNodes(typeof(T).Name);
		bool gotData = false;
		foreach (XmlNode item in xmlNodeList)
		{
			XmlAttribute xmlAttribute = item.Attributes["Abstract"];
			if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
			{
				continue;
			}
			DeepProfiler.Start("DirectXmlToObject.ObjectFromXml<" + typeof(T).Name + ">");
			T val;
			try
			{
				val = DirectXmlToObject.ObjectFromXml<T>(item, doPostLoad: true);
				gotData = true;
			}
			catch (Exception ex)
			{
				Log.Error("Exception loading data from file " + asset.name + ": " + ex);
				continue;
			}
			finally
			{
				DeepProfiler.End();
			}
			yield return val;
		}
		if (!gotData)
		{
			Log.Error("Found no usable data when trying to get " + typeof(T)?.ToString() + "s from file " + asset.name);
		}
	}
}
