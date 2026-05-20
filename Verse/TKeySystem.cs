using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Verse;

public static class TKeySystem
{
	private struct TKeyRef
	{
		public string defName;

		public string defTypeName;

		public XmlNode defRootNode;

		public XmlNode node;

		public string tKey;

		public string tKeyPath;
	}

	private struct PossibleDefInjection
	{
		public string normalizedPath;

		public string path;
	}

	private delegate bool PathMatcher(string path, out string match);

	private static List<TKeyRef> keys = new List<TKeyRef>();

	public static List<string> loadErrors = new List<string>();

	private static HashSet<XmlNode> treatAsList = new HashSet<XmlNode>();

	private static Dictionary<string, string> tKeyToNormalizedTranslationKey = new Dictionary<string, string>();

	private static Dictionary<string, string> translationKeyToTKey = new Dictionary<string, string>();

	public const string AttributeName = "TKey";

	private static bool ShouldUseHardcodedMapping => true;

	public static void Clear()
	{
		keys.Clear();
		tKeyToNormalizedTranslationKey.Clear();
		translationKeyToTKey.Clear();
		loadErrors.Clear();
		treatAsList.Clear();
	}

	public static void Parse(XmlDocument document)
	{
		foreach (XmlNode childNode in document.ChildNodes[0].ChildNodes)
		{
			ParseDefNode(childNode);
		}
	}

	public static void MarkTreatAsList(XmlNode node)
	{
		treatAsList.Add(node);
	}

	public static void BuildMappings()
	{
		if (ShouldUseHardcodedMapping)
		{
			tKeyToNormalizedTranslationKey.AddRange(TKeySystemHardcodedMapping.TKeyToNormalizedTranslationKey);
			translationKeyToTKey.AddRange(TKeySystemHardcodedMapping.TranslationKeyToTKey);
		}
		else
		{
			BuildMappings_Calculate();
		}
	}

	private static void BuildMappings_Calculate()
	{
		Dictionary<string, string> tmpTranslationKeyToTKey = new Dictionary<string, string>();
		foreach (TKeyRef key in keys)
		{
			string normalizedTranslationKey = GetNormalizedTranslationKey(key);
			if (tKeyToNormalizedTranslationKey.TryGetValue(key.tKeyPath, out var value))
			{
				loadErrors.Add("Duplicate TKey: " + key.tKeyPath + " -> NEW=" + normalizedTranslationKey + " | OLD" + value + " - Ignoring old");
			}
			else
			{
				tKeyToNormalizedTranslationKey.Add(key.tKeyPath, normalizedTranslationKey);
				tmpTranslationKeyToTKey.Add(normalizedTranslationKey, key.tKeyPath);
			}
		}
		foreach (string item in keys.Select((TKeyRef k) => k.defTypeName).Distinct())
		{
			DefInjectionUtility.ForEachPossibleDefInjection(GenTypes.GetTypeInAnyAssembly(item), delegate(string suggestedPath, string normalizedPath, bool isCollection, string currentValue, IEnumerable<string> currentValueCollection, bool translationAllowed, bool fullListTranslationAllowed, FieldInfo fieldInfo, Def def)
			{
				if (translationAllowed && !TryGetNormalizedPath(suggestedPath, out var _) && TrySuggestTKeyPath(normalizedPath, out var tKeyPath, tmpTranslationKeyToTKey))
				{
					if (tmpTranslationKeyToTKey.ContainsKey(suggestedPath) && tmpTranslationKeyToTKey[suggestedPath] != tKeyPath)
					{
						Log.Error("Trying to add duplicate TKey with different path. '" + tmpTranslationKeyToTKey[suggestedPath] + "' will be overwritten by '" + tKeyPath + "'.");
					}
					tmpTranslationKeyToTKey.SetOrAdd(suggestedPath, tKeyPath);
				}
			});
		}
		foreach (KeyValuePair<string, string> item2 in tmpTranslationKeyToTKey)
		{
			translationKeyToTKey.Add(item2.Key, item2.Value);
		}
		Compare(tKeyToNormalizedTranslationKey, TKeySystemHardcodedMapping.TKeyToNormalizedTranslationKey, "tKeyToNormalizedTranslationKey(calculated)", "TKeySystemHardcodedMapping.TKeyToNormalizedTranslationKey");
		Compare(translationKeyToTKey, TKeySystemHardcodedMapping.TranslationKeyToTKey, "translationKeyToTKey(calculated)", "TKeySystemHardcodedMapping.TranslationKeyToTKey");
		static void Compare(Dictionary<string, string> a, Dictionary<string, string> b, string aName, string bName)
		{
			foreach (KeyValuePair<string, string> item3 in a)
			{
				if (!b.TryGetValue(item3.Key, out var value2))
				{
					Log.Warning(bName + " is missing " + item3.Key);
				}
				else if (item3.Value != value2)
				{
					Log.Warning(bName + "'s value doesn't match for " + item3.Key + ". Should be " + item3.Value);
				}
			}
			foreach (KeyValuePair<string, string> item4 in b)
			{
				if (!a.ContainsKey(item4.Key))
				{
					Log.Warning(bName + " has " + item4.Key + " but it's not present in " + aName + ". We should most likely remove it from the list.");
				}
			}
		}
	}

	public static bool TryGetNormalizedPath(string tKeyPath, out string normalizedPath)
	{
		return TryFindShortestReplacementPath(tKeyPath, delegate(string path, out string result)
		{
			return tKeyToNormalizedTranslationKey.TryGetValue(path, out result);
		}, out normalizedPath);
	}

	private static bool TryFindShortestReplacementPath(string path, PathMatcher matcher, out string result)
	{
		if (matcher(path, out result))
		{
			return true;
		}
		int num = 100;
		int num2 = path.Length - 1;
		while (true)
		{
			if (num2 > 0 && path[num2] != '.')
			{
				num2--;
				continue;
			}
			if (path[num2] == '.')
			{
				string path2 = path.Substring(0, num2);
				if (matcher(path2, out result))
				{
					result += path.Substring(num2);
					return true;
				}
			}
			num2--;
			num--;
			if (num2 <= 0 || num <= 0)
			{
				break;
			}
		}
		result = null;
		return false;
	}

	public static bool TrySuggestTKeyPath(string translationPath, out string tKeyPath, Dictionary<string, string> lookup = null)
	{
		if (lookup == null)
		{
			lookup = translationKeyToTKey;
		}
		return TryFindShortestReplacementPath(translationPath, delegate(string path, out string result)
		{
			return lookup.TryGetValue(path, out result);
		}, out tKeyPath);
	}

	private static string GetNormalizedTranslationKey(TKeyRef tKeyRef)
	{
		string text = "";
		XmlNode currentNode;
		for (currentNode = tKeyRef.node; currentNode != tKeyRef.defRootNode; currentNode = currentNode.ParentNode)
		{
			text = ((!(currentNode.Name == "li") && !treatAsList.Contains(currentNode.ParentNode)) ? ("." + currentNode.Name + text) : ("." + (from XmlNode n in currentNode.ParentNode.ChildNodes
				where ShouldConsiderNode(n)
				select n).FirstIndexOf((XmlNode n) => n == currentNode) + text));
		}
		return tKeyRef.defName + text;
		static bool ShouldConsiderNode(XmlNode node)
		{
			string text2 = node.Attributes["MayRequire"]?.Value;
			if (text2 != null && !ModLister.AllModsActiveNoSuffix(text2.Split(',')))
			{
				return false;
			}
			string[] array = node.Attributes?["MayRequire"]?.Value.ToLower().Split(',');
			if (!array.NullOrEmpty() && !ModLister.AnyModActiveNoSuffix(array))
			{
				return false;
			}
			return true;
		}
	}

	private static void ParseDefNode(XmlNode node)
	{
		if (ShouldUseHardcodedMapping)
		{
			return;
		}
		string text = null;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "defName")
			{
				text = childNode.InnerText;
				break;
			}
		}
		TKeyRef tKeyRefTemplate;
		if (!string.IsNullOrWhiteSpace(text))
		{
			tKeyRefTemplate = default(TKeyRef);
			tKeyRefTemplate.defName = text;
			tKeyRefTemplate.defTypeName = node.Name;
			tKeyRefTemplate.defRootNode = node;
			CrawlNodesRecursive(node);
		}
		void CrawlNodesRecursive(XmlNode n)
		{
			ProcessNode(n);
			foreach (XmlNode childNode2 in n.ChildNodes)
			{
				CrawlNodesRecursive(childNode2);
			}
		}
		void ProcessNode(XmlNode n)
		{
			XmlAttribute xmlAttribute;
			if (n.Attributes != null && (xmlAttribute = n.Attributes["TKey"]) != null)
			{
				TKeyRef item = tKeyRefTemplate;
				item.tKey = xmlAttribute.Value;
				item.node = n;
				item.tKeyPath = item.defName + "." + item.tKey;
				keys.Add(item);
			}
		}
	}
}
