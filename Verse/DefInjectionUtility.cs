using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld.QuestGen;

namespace Verse;

public static class DefInjectionUtility
{
	public delegate void PossibleDefInjectionTraverser(string suggestedPath, string normalizedPath, bool isCollection, string currentValue, IEnumerable<string> currentValueCollection, bool translationAllowed, bool fullListTranslationAllowed, FieldInfo fieldInfo, Def def);

	private static Dictionary<Type, List<FieldInfo>> cachedFieldsInDeterministicOrder = new Dictionary<Type, List<FieldInfo>>();

	public static void ForEachPossibleDefInjection(Type defType, PossibleDefInjectionTraverser action, ModMetaData onlyFromMod = null)
	{
		foreach (Def item in GenDefDatabase.GetAllDefsInDatabaseForDef(defType))
		{
			if (onlyFromMod == null || (item.modContentPack != null && !(item.modContentPack.PackageId != onlyFromMod.PackageId)))
			{
				ForEachPossibleDefInjectionInDef(item, action);
			}
		}
	}

	private static void ForEachPossibleDefInjectionInDef(Def def, PossibleDefInjectionTraverser action)
	{
		HashSet<object> visited = new HashSet<object>();
		ForEachPossibleDefInjectionInDefRecursive(def, def.defName, def.defName, visited, translationAllowed: true, def, action);
	}

	private static void ForEachPossibleDefInjectionInDefRecursive(object obj, string curNormalizedPath, string curSuggestedPath, HashSet<object> visited, bool translationAllowed, Def def, PossibleDefInjectionTraverser action)
	{
		if (obj == null || obj is Thing || (!obj.GetType().IsValueType && visited.Contains(obj)))
		{
			return;
		}
		visited.Add(obj);
		foreach (FieldInfo item in FieldsInDeterministicOrder(obj.GetType()))
		{
			object value = item.GetValue(obj);
			bool flag = translationAllowed && !item.HasAttribute<NoTranslateAttribute>() && !item.HasAttribute<UnsavedAttribute>();
			if (value is Def)
			{
				continue;
			}
			if (typeof(string).IsAssignableFrom(item.FieldType))
			{
				string currentValue = (string)value;
				string text = curNormalizedPath + "." + item.Name;
				string suggestedPath = curSuggestedPath + "." + item.Name;
				if (TKeySystem.TrySuggestTKeyPath(text, out var tKeyPath))
				{
					suggestedPath = tKeyPath;
				}
				action(suggestedPath, text, isCollection: false, currentValue, null, flag, fullListTranslationAllowed: false, item, def);
			}
			else if (value is IEnumerable<string> currentValueCollection)
			{
				bool flag2 = item.HasAttribute<TranslationCanChangeCountAttribute>();
				string text2 = curNormalizedPath + "." + item.Name;
				string suggestedPath2 = curSuggestedPath + "." + item.Name;
				if (TKeySystem.TrySuggestTKeyPath(text2, out var tKeyPath2))
				{
					suggestedPath2 = tKeyPath2;
				}
				action(suggestedPath2, text2, isCollection: true, null, currentValueCollection, flag, flag && flag2, item, def);
			}
			else if (value is IEnumerable enumerable)
			{
				int num = 0;
				foreach (object item2 in enumerable)
				{
					if (item2 != null && !(item2 is Def) && GenTypes.IsCustomType(item2.GetType()))
					{
						string text3 = TranslationHandleUtility.GetBestHandleWithIndexForListElement(enumerable, item2);
						if (text3.NullOrEmpty())
						{
							text3 = num.ToString();
						}
						string curNormalizedPath2 = curNormalizedPath + "." + item.Name + "." + num;
						string curSuggestedPath2 = curSuggestedPath + "." + item.Name + "." + text3;
						ForEachPossibleDefInjectionInDefRecursive(item2, curNormalizedPath2, curSuggestedPath2, visited, flag, def, action);
					}
					num++;
				}
			}
			else if (value != null && GenTypes.IsCustomType(value.GetType()))
			{
				string curNormalizedPath3 = curNormalizedPath + "." + item.Name;
				string curSuggestedPath3 = curSuggestedPath + "." + item.Name;
				ForEachPossibleDefInjectionInDefRecursive(value, curNormalizedPath3, curSuggestedPath3, visited, flag, def, action);
			}
		}
	}

	public static bool ShouldCheckMissingInjection(string str, FieldInfo fi, Def def)
	{
		if (def.generated)
		{
			return false;
		}
		if (str.NullOrEmpty())
		{
			return false;
		}
		if (fi.HasAttribute<NoTranslateAttribute>() || fi.HasAttribute<UnsavedAttribute>() || fi.HasAttribute<MayTranslateAttribute>())
		{
			return false;
		}
		if (fi.HasAttribute<MustTranslate_SlateRefAttribute>())
		{
			return SlateRefUtility.MustTranslate(str, fi);
		}
		if (!fi.HasAttribute<MustTranslateAttribute>())
		{
			return str.Contains(' ');
		}
		return true;
	}

	private static List<FieldInfo> FieldsInDeterministicOrder(Type type)
	{
		if (cachedFieldsInDeterministicOrder.TryGetValue(type, out var value))
		{
			return value;
		}
		List<FieldInfo> list = (from x in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			orderby x.HasAttribute<UnsavedAttribute>() || x.HasAttribute<NoTranslateAttribute>(), x.Name == "label" descending, x.Name == "description" descending, x.Name
			select x).ToList();
		cachedFieldsInDeterministicOrder.Add(type, list);
		return list;
	}
}
