using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace Verse;

public static class DirectXmlCrossRefLoader
{
	private abstract class WantedRef
	{
		public object wanter;

		public abstract bool TryResolve(FailMode failReportMode);

		public virtual void Apply()
		{
		}
	}

	private class WantedRefForObject : WantedRef
	{
		private readonly FieldInfo fi;

		private readonly string defName;

		private readonly string mayRequireMod;

		private readonly string[] mayRequireAnyMod;

		private readonly Type overrideFieldType;

		private Def resolvedDef;

		private bool BadCrossRefAllowed
		{
			get
			{
				if (!mayRequireMod.NullOrEmpty() && !ModLister.AllModsActiveNoSuffix(mayRequireMod.Split(',')))
				{
					return true;
				}
				if (!mayRequireAnyMod.NullOrEmpty() && !ModLister.AnyModActiveNoSuffix(mayRequireAnyMod))
				{
					return true;
				}
				return false;
			}
		}

		public WantedRefForObject(object wanter, FieldInfo fi, string targetDefName, string mayRequireMod = null, string mayRequireAnyMod = null, Type overrideFieldType = null)
		{
			base.wanter = wanter;
			this.fi = fi;
			defName = targetDefName;
			this.mayRequireMod = mayRequireMod;
			this.overrideFieldType = overrideFieldType;
			this.mayRequireAnyMod = mayRequireAnyMod?.ToLower().Split(',');
		}

		public override bool TryResolve(FailMode failReportMode)
		{
			if (fi == null)
			{
				Log.Error("Trying to resolve null field for def named " + defName.ToStringSafe());
				return false;
			}
			Type type = overrideFieldType ?? fi.FieldType;
			resolvedDef = GenDefDatabase.GetDefSilentFail(type, defName);
			if (MistypedMayRequire(mayRequireMod))
			{
				Log.Error("Faulty MayRequire at def " + defName.ToStringSafe() + ": " + mayRequireMod);
			}
			if (!mayRequireAnyMod.NullOrEmpty())
			{
				string[] array = mayRequireAnyMod;
				foreach (string text in array)
				{
					if (MistypedMayRequire(text))
					{
						Log.Error("Faulty MayRequire at def " + defName.ToStringSafe() + ": " + text);
					}
				}
			}
			if (resolvedDef == null)
			{
				if (failReportMode == FailMode.LogErrors && !BadCrossRefAllowed)
				{
					Log.Error("Could not resolve cross-reference: No " + type?.ToString() + " named " + defName.ToStringSafe() + " found to give to " + wanter.GetType()?.ToString() + " " + wanter.ToStringSafe());
				}
				return false;
			}
			if (resolvedDef is SoundDef { isUndefined: not false })
			{
				Log.Warning("Could not resolve cross-reference: No " + type?.ToString() + " named " + defName.ToStringSafe() + " found to give to " + wanter.GetType()?.ToString() + " " + wanter.ToStringSafe() + " (using undefined sound instead)");
			}
			fi.SetValue(wanter, resolvedDef);
			return true;
		}
	}

	private class WantedRefForList<T> : WantedRef
	{
		private List<string> defNames = new List<string>();

		private List<string> mayRequireMods;

		private Dictionary<string, List<string>> mayRequireModsAny;

		private object debugWanterInfo;

		public WantedRefForList(object wanter, object debugWanterInfo)
		{
			base.wanter = wanter;
			this.debugWanterInfo = debugWanterInfo;
		}

		public void AddWantedListEntry(string newTargetDefName, string mayRequireMod = null, string mayRequireAnyMod = null)
		{
			if (!mayRequireMod.NullOrEmpty() && mayRequireMods == null)
			{
				mayRequireMods = new List<string>();
				for (int i = 0; i < defNames.Count; i++)
				{
					mayRequireMods.Add(null);
				}
			}
			if (!mayRequireAnyMod.NullOrEmpty() && mayRequireModsAny == null)
			{
				mayRequireModsAny = new Dictionary<string, List<string>>();
				for (int j = 0; j < defNames.Count; j++)
				{
					mayRequireModsAny.Add(defNames[j], new List<string>());
				}
			}
			defNames.Add(newTargetDefName);
			if (mayRequireMods != null)
			{
				mayRequireMods.Add(mayRequireMod);
			}
			if (mayRequireModsAny == null || mayRequireAnyMod == null)
			{
				return;
			}
			string[] array = mayRequireAnyMod.ToLower().Split(',');
			foreach (string text in array)
			{
				if (mayRequireModsAny.TryGetValue(newTargetDefName, out var value))
				{
					value.Add(text.Trim());
					continue;
				}
				mayRequireModsAny.Add(newTargetDefName, new List<string> { text.Trim() });
			}
		}

		public override bool TryResolve(FailMode failReportMode)
		{
			bool flag = false;
			for (int i = 0; i < defNames.Count; i++)
			{
				bool flag2 = mayRequireMods != null && i < mayRequireMods.Count && !mayRequireMods[i].NullOrEmpty() && !ModLister.AllModsActiveNoSuffix(mayRequireMods[i].Split(','));
				if (mayRequireModsAny != null && mayRequireModsAny.TryGetValue(defNames[i], out var value) && !ModLister.AnyModActiveNoSuffix(value))
				{
					flag2 = true;
				}
				if (mayRequireMods != null && i < mayRequireMods.Count && MistypedMayRequire(mayRequireMods[i]))
				{
					Log.Error("Faulty MayRequire: " + mayRequireMods[i]);
				}
				T val = TryResolveDef<T>(defNames[i], (!flag2) ? failReportMode : FailMode.Silent, debugWanterInfo);
				if (val != null)
				{
					((List<T>)wanter).Add(val);
					defNames.RemoveAt(i);
					if (mayRequireMods != null && i < mayRequireMods.Count)
					{
						mayRequireMods.RemoveAt(i);
					}
					i--;
				}
				else
				{
					flag = true;
				}
			}
			return !flag;
		}
	}

	private class WantedRefForDictionary<K, V> : WantedRef
	{
		private List<XmlNode> wantedDictRefs = new List<XmlNode>();

		private object debugWanterInfo;

		private List<Pair<object, object>> makingData = new List<Pair<object, object>>();

		public WantedRefForDictionary(object wanter, object debugWanterInfo)
		{
			base.wanter = wanter;
			this.debugWanterInfo = debugWanterInfo;
		}

		public void AddWantedDictEntry(XmlNode entryNode)
		{
			wantedDictRefs.Add(entryNode);
		}

		public override bool TryResolve(FailMode failReportMode)
		{
			failReportMode = FailMode.LogErrors;
			bool flag = GenTypes.IsDef(typeof(K));
			bool flag2 = GenTypes.IsDef(typeof(V));
			foreach (XmlNode wantedDictRef in wantedDictRefs)
			{
				XmlNode xmlNode = wantedDictRef["key"];
				XmlNode xmlNode2 = wantedDictRef["value"];
				string text = xmlNode?.InnerText;
				string text2 = xmlNode2?.InnerText;
				object first;
				object second;
				if (text == null || text2 == null)
				{
					if (failReportMode == FailMode.LogErrors)
					{
						string text3 = "Missing 'key' and/or 'value'.";
						if (debugWanterInfo != null)
						{
							text3 = text3 + " (wanter=" + debugWanterInfo.ToStringSafe() + ")";
						}
						Log.Error(text3);
					}
					first = default(K);
					second = default(V);
				}
				else
				{
					first = ((!flag) ? xmlNode : ((object)TryResolveDef<K>(text, failReportMode, debugWanterInfo)));
					second = ((!flag2) ? xmlNode2 : ((object)TryResolveDef<V>(text2, failReportMode, debugWanterInfo)));
				}
				makingData.Add(new Pair<object, object>(first, second));
			}
			return true;
		}

		public override void Apply()
		{
			Dictionary<K, V> dictionary = (Dictionary<K, V>)wanter;
			dictionary.Clear();
			foreach (Pair<object, object> makingDatum in makingData)
			{
				try
				{
					object obj = makingDatum.First;
					object obj2 = makingDatum.Second;
					if (obj is XmlNode)
					{
						obj = DirectXmlToObject.ObjectFromXml<K>(obj as XmlNode, doPostLoad: true);
					}
					if (obj2 is XmlNode)
					{
						obj2 = DirectXmlToObject.ObjectFromXml<V>(obj2 as XmlNode, doPostLoad: true);
					}
					dictionary.Add((K)obj, (V)obj2);
				}
				catch
				{
					Log.Error("Failed to load key/value pair: " + makingDatum.First?.ToString() + ", " + makingDatum.Second);
				}
			}
		}
	}

	private static List<WantedRef> wantedRefs = new List<WantedRef>();

	private static Dictionary<object, WantedRef> wantedListDictRefs = new Dictionary<object, WantedRef>();

	public static bool LoadingInProgress => wantedRefs.Count > 0;

	public static bool MistypedMayRequire(string mayRequireMod)
	{
		if (!Application.isEditor)
		{
			return false;
		}
		if (mayRequireMod.NullOrEmpty())
		{
			return false;
		}
		if (mayRequireMod.Contains(','))
		{
			string[] array = mayRequireMod.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!ExpansionFound(array[i]))
				{
					return true;
				}
			}
		}
		else if (!ExpansionFound(mayRequireMod))
		{
			return true;
		}
		return false;
		static bool ExpansionFound(string modID)
		{
			for (int j = 0; j < ModContentPack.ProductPackageIDs.Length; j++)
			{
				if (modID.EqualsIgnoreCase(ModContentPack.ProductPackageIDs[j]))
				{
					return true;
				}
			}
			return false;
		}
	}

	public static void RegisterObjectWantsCrossRef(object wanter, FieldInfo fi, string targetDefName, string mayRequireMod = null, string mayRequireAnyMod = null, Type assumeFieldType = null)
	{
		DeepProfiler.Start("RegisterObjectWantsCrossRef (object, FieldInfo, string)");
		try
		{
			if (wanter.GetType().IsValueType)
			{
				Log.Error($"Cannot use value types for object cross reference. {wanter.GetType()} is a value type but wants a cross reference to {fi.FieldType} {targetDefName} via field {fi.Name}.");
			}
			WantedRefForObject item = new WantedRefForObject(wanter, fi, targetDefName, mayRequireMod, mayRequireAnyMod, assumeFieldType);
			wantedRefs.Add(item);
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public static void RegisterObjectWantsCrossRef(object wanter, string fieldName, string targetDefName, string mayRequireMod = null, string mayRequireAnyMod = null, Type overrideFieldType = null)
	{
		DeepProfiler.Start("RegisterObjectWantsCrossRef (object,string,string)");
		try
		{
			FieldInfo field = wanter.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (wanter.GetType().IsValueType)
			{
				Log.Error($"Cannot use value types for object cross reference. {wanter.GetType()} is a value type but wants a cross reference to {field?.FieldType} {targetDefName} via field {field?.Name ?? fieldName}.");
			}
			WantedRefForObject item = new WantedRefForObject(wanter, field, targetDefName, mayRequireMod, mayRequireAnyMod, overrideFieldType);
			wantedRefs.Add(item);
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public static void RegisterObjectWantsCrossRef(object wanter, string fieldName, XmlNode parentNode, string mayRequireMod = null, string mayRequireAnyMod = null, Type overrideFieldType = null)
	{
		DeepProfiler.Start("RegisterObjectWantsCrossRef (object,string,XmlNode)");
		try
		{
			if (wanter.GetType().IsValueType)
			{
				Log.Error($"Cannot use value types for object cross reference. {wanter.GetType()} is a value type but wants a cross reference to {parentNode.Name} via field {fieldName}.");
			}
			string mayRequireMod2 = mayRequireMod ?? parentNode.Attributes?["MayRequire"]?.Value.ToLower();
			string mayRequireAnyMod2 = mayRequireAnyMod ?? parentNode.Attributes?["MayRequireAnyOf"]?.Value.ToLower();
			WantedRefForObject item = new WantedRefForObject(wanter, wanter.GetType().GetField(fieldName), parentNode.Name, mayRequireMod2, mayRequireAnyMod2, overrideFieldType);
			wantedRefs.Add(item);
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public static void RegisterListWantsCrossRef<T>(List<T> wanterList, string targetDefName, object debugWanterInfo = null, string mayRequireMod = null, string mayRequireAnyMod = null)
	{
		DeepProfiler.Start("RegisterListWantsCrossRef");
		try
		{
			WantedRefForList<T> wantedRefForList = null;
			if (!wantedListDictRefs.TryGetValue(wanterList, out var value))
			{
				wantedRefForList = new WantedRefForList<T>(wanterList, debugWanterInfo);
				wantedListDictRefs.Add(wanterList, wantedRefForList);
				wantedRefs.Add(wantedRefForList);
			}
			else
			{
				wantedRefForList = (WantedRefForList<T>)value;
			}
			wantedRefForList.AddWantedListEntry(targetDefName, mayRequireMod, mayRequireAnyMod);
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public static void RegisterDictionaryWantsCrossRef<K, V>(Dictionary<K, V> wanterDict, XmlNode entryNode, object debugWanterInfo = null)
	{
		DeepProfiler.Start("RegisterDictionaryWantsCrossRef");
		try
		{
			WantedRefForDictionary<K, V> wantedRefForDictionary = null;
			if (!wantedListDictRefs.TryGetValue(wanterDict, out var value))
			{
				wantedRefForDictionary = new WantedRefForDictionary<K, V>(wanterDict, debugWanterInfo);
				wantedRefs.Add(wantedRefForDictionary);
				wantedListDictRefs.Add(wanterDict, wantedRefForDictionary);
			}
			else
			{
				wantedRefForDictionary = (WantedRefForDictionary<K, V>)value;
			}
			wantedRefForDictionary.AddWantedDictEntry(entryNode);
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public static T TryResolveDef<T>(string defName, FailMode failReportMode, object debugWanterInfo = null)
	{
		DeepProfiler.Start("TryResolveDef");
		try
		{
			T val = (T)(object)GenDefDatabase.GetDefSilentFail(typeof(T), defName);
			if (val != null)
			{
				return val;
			}
			if (failReportMode == FailMode.LogErrors)
			{
				string text = "Could not resolve cross-reference to " + typeof(T)?.ToString() + " named " + defName.ToStringSafe();
				if (debugWanterInfo != null)
				{
					text = text + " (wanter=" + debugWanterInfo.ToStringSafe() + ")";
				}
				Log.Error(text);
			}
			return default(T);
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public static void Clear()
	{
		DeepProfiler.Start("Clear");
		try
		{
			wantedRefs.Clear();
			wantedListDictRefs.Clear();
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	public static void ResolveAllWantedCrossReferences(FailMode failReportMode)
	{
		DeepProfiler.Start("ResolveAllWantedCrossReferences");
		try
		{
			HashSet<WantedRef> resolvedRefs = new HashSet<WantedRef>();
			object resolvedRefsLock = new object();
			DeepProfiler.enabled = false;
			GenThreading.ParallelForEach(wantedRefs, delegate(WantedRef wantedRef)
			{
				if (wantedRef.TryResolve(failReportMode))
				{
					lock (resolvedRefsLock)
					{
						resolvedRefs.Add(wantedRef);
					}
				}
			});
			foreach (WantedRef item in resolvedRefs)
			{
				item.Apply();
			}
			wantedRefs.RemoveAll((WantedRef x) => resolvedRefs.Contains(x));
			DeepProfiler.enabled = true;
		}
		finally
		{
			DeepProfiler.End();
		}
	}
}
