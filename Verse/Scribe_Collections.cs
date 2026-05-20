using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using RimWorld.Planet;

namespace Verse;

public static class Scribe_Collections
{
	private static MethodInfo lookListOfListsInfo;

	public static void Look<T>(ref List<T> list, string label, bool saveDestroyedThings, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
	{
		if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
		{
			if (lookListOfListsInfo == null)
			{
				lookListOfListsInfo = typeof(Scribe_Collections)?.GetMethod("LookListOfLists", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			}
			MethodInfo methodInfo = lookListOfListsInfo?.MakeGenericMethod(typeof(T).GetGenericArguments()[0]);
			object[] array = new object[5] { list, saveDestroyedThings, label, lookMode, ctorArgs };
			methodInfo.Invoke(null, array);
			list = (List<T>)array[0];
		}
		else
		{
			Look(ref list, saveDestroyedThings, label, lookMode, ctorArgs);
		}
	}

	public static void Look<T>(ref List<T> list, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
	{
		Look(ref list, label, saveDestroyedThings: false, lookMode, ctorArgs);
	}

	private static void LookListOfLists<T>(ref List<List<T>> list, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
	{
		if (lookMode == LookMode.Undefined && !Scribe_Universal.TryResolveLookMode(typeof(List<T>), out lookMode))
		{
			Log.Error("LookList call with a list of " + typeof(List<T>)?.ToString() + " must have lookMode set explicitly.");
			return;
		}
		if (Scribe.EnterNode(label))
		{
			try
			{
				if (Scribe.mode == LoadSaveMode.Saving)
				{
					if (list != null)
					{
						foreach (List<T> item in list)
						{
							List<T> list2 = item;
							Look(ref list2, saveDestroyedThings, "li", lookMode, ctorArgs);
						}
						return;
					}
					Scribe.saver.WriteAttribute("IsNull", "True");
				}
				else if (Scribe.mode == LoadSaveMode.LoadingVars)
				{
					XmlNode curXmlParent = Scribe.loader.curXmlParent;
					XmlAttribute xmlAttribute = curXmlParent.Attributes["IsNull"];
					if (xmlAttribute == null || !xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
					{
						list = new List<List<T>>(curXmlParent.ChildNodes.Count);
						int num = 0;
						{
							foreach (XmlNode childNode in curXmlParent.ChildNodes)
							{
								List<T> list3 = new List<T>(childNode.ChildNodes.Count);
								Look(ref list3, saveDestroyedThings, num.ToString(), lookMode, ctorArgs);
								list.Add(list3);
								num++;
							}
							return;
						}
					}
					if (lookMode == LookMode.Reference)
					{
						Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, null);
					}
					list = null;
				}
				else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						List<T> list4 = list[i];
						Look(ref list4, saveDestroyedThings, i.ToString(), lookMode, ctorArgs);
						list[i] = list4;
					}
				}
				return;
			}
			finally
			{
				Scribe.ExitNode();
			}
		}
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (lookMode == LookMode.Reference)
			{
				Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, label);
			}
			list = null;
		}
	}

	private static void Look<T>(ref List<T> list, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
	{
		if (lookMode == LookMode.Undefined && !Scribe_Universal.TryResolveLookMode(typeof(T), out lookMode))
		{
			Log.Error("LookList call with a list of " + typeof(T)?.ToString() + " must have lookMode set explicitly.");
			return;
		}
		if (Scribe.EnterNode(label))
		{
			try
			{
				if (Scribe.mode == LoadSaveMode.Saving)
				{
					if (list != null)
					{
						foreach (T item8 in list)
						{
							if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
							{
								throw new InvalidOperationException("This case should be impossible; it should be calling the list-of-lists overload.");
							}
							switch (lookMode)
							{
							case LookMode.Value:
							{
								T value4 = item8;
								Scribe_Values.Look(ref value4, "li", default(T), forceSave: true);
								break;
							}
							case LookMode.LocalTargetInfo:
							{
								LocalTargetInfo value3 = (LocalTargetInfo)(object)item8;
								Scribe_TargetInfo.Look(ref value3, saveDestroyedThings, "li");
								break;
							}
							case LookMode.TargetInfo:
							{
								TargetInfo value2 = (TargetInfo)(object)item8;
								Scribe_TargetInfo.Look(ref value2, saveDestroyedThings, "li");
								break;
							}
							case LookMode.GlobalTargetInfo:
							{
								GlobalTargetInfo value = (GlobalTargetInfo)(object)item8;
								Scribe_TargetInfo.Look(ref value, saveDestroyedThings, "li");
								break;
							}
							case LookMode.Def:
							{
								Def value5 = (Def)(object)item8;
								Scribe_Defs.Look(ref value5, "li");
								break;
							}
							case LookMode.BodyPart:
							{
								BodyPartRecord part = (BodyPartRecord)(object)item8;
								Scribe_BodyParts.Look(ref part, "li");
								break;
							}
							case LookMode.Deep:
							{
								T target = item8;
								Scribe_Deep.Look(ref target, saveDestroyedThings, "li", ctorArgs);
								break;
							}
							case LookMode.Reference:
							{
								if (item8 != null && !(item8 is ILoadReferenceable))
								{
									throw new InvalidOperationException("Cannot save reference to " + item8?.GetType()?.ToStringSafe() + " item if it is not ILoadReferenceable");
								}
								ILoadReferenceable refee = item8 as ILoadReferenceable;
								Scribe_References.Look(ref refee, "li", saveDestroyedThings);
								break;
							}
							}
						}
						return;
					}
					Scribe.saver.WriteAttribute("IsNull", "True");
				}
				else if (Scribe.mode == LoadSaveMode.LoadingVars)
				{
					XmlNode curXmlParent = Scribe.loader.curXmlParent;
					XmlAttribute xmlAttribute = curXmlParent.Attributes["IsNull"];
					if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
					{
						if (lookMode == LookMode.Reference)
						{
							Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, null);
						}
						list = null;
					}
					else
					{
						switch (lookMode)
						{
						case LookMode.Value:
							list = new List<T>(curXmlParent.ChildNodes.Count);
							{
								foreach (XmlNode childNode in curXmlParent.ChildNodes)
								{
									T item = ScribeExtractor.ValueFromNode(childNode, default(T));
									list.Add(item);
								}
								break;
							}
						case LookMode.Deep:
							list = new List<T>(curXmlParent.ChildNodes.Count);
							{
								foreach (XmlNode childNode2 in curXmlParent.ChildNodes)
								{
									T item7 = ScribeExtractor.SaveableFromNode<T>(childNode2, ctorArgs);
									list.Add(item7);
								}
								break;
							}
						case LookMode.Def:
							list = new List<T>(curXmlParent.ChildNodes.Count);
							{
								foreach (XmlNode childNode3 in curXmlParent.ChildNodes)
								{
									T item6 = ScribeExtractor.DefFromNodeUnsafe<T>(childNode3);
									list.Add(item6);
								}
								break;
							}
						case LookMode.BodyPart:
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num4 = 0;
							{
								foreach (XmlNode childNode4 in curXmlParent.ChildNodes)
								{
									T item5 = (T)(object)ScribeExtractor.BodyPartFromNode(childNode4, num4.ToString(), null);
									list.Add(item5);
									num4++;
								}
								break;
							}
						}
						case LookMode.LocalTargetInfo:
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num3 = 0;
							{
								foreach (XmlNode childNode5 in curXmlParent.ChildNodes)
								{
									T item4 = (T)(object)ScribeExtractor.LocalTargetInfoFromNode(childNode5, num3.ToString(), LocalTargetInfo.Invalid);
									list.Add(item4);
									num3++;
								}
								break;
							}
						}
						case LookMode.TargetInfo:
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num2 = 0;
							{
								foreach (XmlNode childNode6 in curXmlParent.ChildNodes)
								{
									T item3 = (T)(object)ScribeExtractor.TargetInfoFromNode(childNode6, num2.ToString(), TargetInfo.Invalid);
									list.Add(item3);
									num2++;
								}
								break;
							}
						}
						case LookMode.GlobalTargetInfo:
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num = 0;
							{
								foreach (XmlNode childNode7 in curXmlParent.ChildNodes)
								{
									T item2 = (T)(object)ScribeExtractor.GlobalTargetInfoFromNode(childNode7, num.ToString(), GlobalTargetInfo.Invalid);
									list.Add(item2);
									num++;
								}
								break;
							}
						}
						case LookMode.Reference:
						{
							List<string> list2 = new List<string>(curXmlParent.ChildNodes.Count);
							foreach (XmlNode childNode8 in curXmlParent.ChildNodes)
							{
								list2.Add(childNode8.InnerText);
							}
							Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(list2, "");
							break;
						}
						}
					}
				}
				else if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
				{
					switch (lookMode)
					{
					case LookMode.Reference:
						list = Scribe.loader.crossRefs.TakeResolvedRefList<T>("");
						break;
					case LookMode.LocalTargetInfo:
						if (list != null)
						{
							for (int j = 0; j < list.Count; j++)
							{
								list[j] = (T)(object)ScribeExtractor.ResolveLocalTargetInfo((LocalTargetInfo)(object)list[j], j.ToString());
							}
						}
						break;
					case LookMode.TargetInfo:
						if (list != null)
						{
							for (int k = 0; k < list.Count; k++)
							{
								list[k] = (T)(object)ScribeExtractor.ResolveTargetInfo((TargetInfo)(object)list[k], k.ToString());
							}
						}
						break;
					case LookMode.GlobalTargetInfo:
						if (list != null)
						{
							for (int i = 0; i < list.Count; i++)
							{
								list[i] = (T)(object)ScribeExtractor.ResolveGlobalTargetInfo((GlobalTargetInfo)(object)list[i], i.ToString());
							}
						}
						break;
					}
				}
				return;
			}
			finally
			{
				Scribe.ExitNode();
			}
		}
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (lookMode == LookMode.Reference)
			{
				Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, label);
			}
			list = null;
		}
	}

	public static void Look<K, V>(ref Dictionary<K, V> dict, string label, LookMode keyLookMode = LookMode.Undefined, LookMode valueLookMode = LookMode.Undefined)
	{
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			bool num = keyLookMode == LookMode.Reference;
			bool flag = valueLookMode == LookMode.Reference;
			if (num != flag)
			{
				Log.Error("You need to provide working lists for the keys and values in order to be able to load such dictionary. label=" + label);
			}
		}
		List<K> keysWorkingList = null;
		List<V> valuesWorkingList = null;
		Look(ref dict, label, keyLookMode, valueLookMode, ref keysWorkingList, ref valuesWorkingList);
	}

	private static void BuildDictionary<K, V>(Dictionary<K, V> dict, List<K> keysWorkingList, List<V> valuesWorkingList, string label, bool logNullErrors)
	{
		if (dict == null)
		{
			return;
		}
		if (keysWorkingList == null)
		{
			Log.Error("Cannot fill dictionary because there are no keys. label=" + label);
			return;
		}
		if (valuesWorkingList == null)
		{
			Log.Error("Cannot fill dictionary because there are no values. label=" + label);
			return;
		}
		if (keysWorkingList.Count != valuesWorkingList.Count)
		{
			Log.Error("Keys count does not match the values count while loading a dictionary (maybe keys and values were resolved during different passes?). Some elements will be skipped. keys=" + keysWorkingList.Count + ", values=" + valuesWorkingList.Count + ", label=" + label);
		}
		int num = Math.Min(keysWorkingList.Count, valuesWorkingList.Count);
		for (int i = 0; i < num; i++)
		{
			if (keysWorkingList[i] == null)
			{
				if (logNullErrors)
				{
					Log.Error("Null key while loading dictionary of " + typeof(K)?.ToString() + " and " + typeof(V)?.ToString() + ". label=" + label);
				}
				continue;
			}
			try
			{
				if (dict.TryGetValue(keysWorkingList[i], out var value))
				{
					if (!object.Equals(value, valuesWorkingList[i]))
					{
						throw new InvalidOperationException("Tried to add different values for the same key.");
					}
				}
				else
				{
					dict.Add(keysWorkingList[i], valuesWorkingList[i]);
				}
			}
			catch (OutOfMemoryException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				Log.Error("Exception in LookDictionary(label=" + label + "): " + ex2);
			}
		}
	}

	public static void Look<K, V>(ref Dictionary<K, V> dict, string label, LookMode keyLookMode, LookMode valueLookMode, ref List<K> keysWorkingList, ref List<V> valuesWorkingList, bool logNullErrors = true, bool saveDestroyedKeys = false, bool saveDestroyedValues = false)
	{
		if (Scribe.EnterNode(label))
		{
			try
			{
				if (Scribe.mode == LoadSaveMode.Saving && dict == null)
				{
					Scribe.saver.WriteAttribute("IsNull", "True");
				}
				else
				{
					if (Scribe.mode == LoadSaveMode.Saving && typeof(K).IsGenericType && typeof(K).GetGenericTypeDefinition() == typeof(List<>))
					{
						throw new InvalidOperationException("Cannot sensibly save a dictionary with a key type which is a list.");
					}
					if (Scribe.mode == LoadSaveMode.LoadingVars)
					{
						XmlAttribute xmlAttribute = Scribe.loader.curXmlParent.Attributes["IsNull"];
						if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
						{
							dict = null;
						}
						else
						{
							dict = new Dictionary<K, V>();
						}
					}
					if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
					{
						keysWorkingList = new List<K>();
						valuesWorkingList = new List<V>();
						if (Scribe.mode == LoadSaveMode.Saving && dict != null)
						{
							foreach (KeyValuePair<K, V> item in dict)
							{
								keysWorkingList.Add(item.Key);
								valuesWorkingList.Add(item.Value);
							}
						}
					}
					if (Scribe.mode == LoadSaveMode.Saving || dict != null)
					{
						Look(ref keysWorkingList, "keys", saveDestroyedKeys, keyLookMode);
						Look(ref valuesWorkingList, "values", saveDestroyedValues, valueLookMode);
					}
					if (Scribe.mode == LoadSaveMode.Saving)
					{
						if (keysWorkingList != null)
						{
							keysWorkingList.Clear();
							keysWorkingList = null;
						}
						if (valuesWorkingList != null)
						{
							valuesWorkingList.Clear();
							valuesWorkingList = null;
						}
					}
					bool flag = keyLookMode == LookMode.Reference || valueLookMode == LookMode.Reference;
					if ((flag && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (!flag && Scribe.mode == LoadSaveMode.LoadingVars))
					{
						BuildDictionary(dict, keysWorkingList, valuesWorkingList, label, logNullErrors);
					}
					if (Scribe.mode == LoadSaveMode.PostLoadInit)
					{
						if (keysWorkingList != null)
						{
							keysWorkingList.Clear();
							keysWorkingList = null;
						}
						if (valuesWorkingList != null)
						{
							valuesWorkingList.Clear();
							valuesWorkingList = null;
						}
					}
				}
				return;
			}
			finally
			{
				Scribe.ExitNode();
			}
		}
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			dict = null;
		}
	}

	public static void Look<T>(ref HashSet<T> valueHashSet, string label, LookMode lookMode = LookMode.Undefined)
	{
		Look(ref valueHashSet, saveDestroyedThings: false, label, lookMode);
	}

	public static void Look<T>(ref HashSet<T> valueHashSet, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined)
	{
		List<T> list = null;
		if (Scribe.mode == LoadSaveMode.Saving && valueHashSet != null)
		{
			list = new List<T>();
			foreach (T item in valueHashSet)
			{
				list.Add(item);
			}
		}
		Look(ref list, saveDestroyedThings, label, lookMode);
		if ((lookMode != LookMode.Reference || Scribe.mode != LoadSaveMode.ResolvingCrossRefs) && (lookMode == LookMode.Reference || Scribe.mode != LoadSaveMode.LoadingVars))
		{
			return;
		}
		if (list == null)
		{
			valueHashSet = null;
			return;
		}
		valueHashSet = new HashSet<T>();
		for (int i = 0; i < list.Count; i++)
		{
			valueHashSet.Add(list[i]);
		}
	}

	public static void Look<T>(ref Stack<T> valueStack, string label, LookMode lookMode = LookMode.Undefined)
	{
		List<T> list = null;
		if (Scribe.mode == LoadSaveMode.Saving && valueStack != null)
		{
			list = new List<T>();
			foreach (T item in valueStack)
			{
				list.Add(item);
			}
		}
		Look(ref list, label, lookMode);
		if ((lookMode != LookMode.Reference || Scribe.mode != LoadSaveMode.ResolvingCrossRefs) && (lookMode == LookMode.Reference || Scribe.mode != LoadSaveMode.LoadingVars))
		{
			return;
		}
		if (list == null)
		{
			valueStack = null;
			return;
		}
		valueStack = new Stack<T>();
		for (int i = 0; i < list.Count; i++)
		{
			valueStack.Push(list[i]);
		}
	}

	public static void Look<T>(ref Queue<T> valueQueue, string label, LookMode lookMode = LookMode.Undefined)
	{
		List<T> list = null;
		if (Scribe.mode == LoadSaveMode.Saving && valueQueue != null)
		{
			list = new List<T>();
			foreach (T item in valueQueue)
			{
				list.Add(item);
			}
		}
		Look(ref list, label, lookMode);
		if ((lookMode != LookMode.Reference || Scribe.mode != LoadSaveMode.ResolvingCrossRefs) && (lookMode == LookMode.Reference || Scribe.mode != LoadSaveMode.LoadingVars))
		{
			return;
		}
		if (list == null)
		{
			valueQueue = null;
			return;
		}
		valueQueue = new Queue<T>();
		for (int i = 0; i < list.Count; i++)
		{
			valueQueue.Enqueue(list[i]);
		}
	}
}
