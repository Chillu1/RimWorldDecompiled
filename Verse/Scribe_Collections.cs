using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Verse
{
	public static class Scribe_Collections
	{
		public static void Look<T>(ref List<T> list, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
		{
			Look(ref list, saveDestroyedThings: false, label, lookMode, ctorArgs);
		}

		public static void Look<T>(ref List<T> list, bool saveDestroyedThings, string label, LookMode lookMode = LookMode.Undefined, params object[] ctorArgs)
		{
			if (lookMode == LookMode.Undefined && !Scribe_Universal.TryResolveLookMode(typeof(T), out lookMode))
			{
				Log.Error(string.Concat("LookList call with a list of ", typeof(T), " must have lookMode set explicitly."));
			}
			else if (Scribe.EnterNode(label))
			{
				try
				{
					if (Scribe.mode == LoadSaveMode.Saving)
					{
						if (list == null)
						{
							Scribe.saver.WriteAttribute("IsNull", "True");
							return;
						}
						foreach (T item8 in list)
						{
							switch (lookMode)
							{
							case LookMode.Value:
							{
								T value5 = item8;
								Scribe_Values.Look(ref value5, "li", default(T), forceSave: true);
								break;
							}
							case LookMode.LocalTargetInfo:
							{
								LocalTargetInfo value4 = (LocalTargetInfo)(object)item8;
								Scribe_TargetInfo.Look(ref value4, saveDestroyedThings, "li");
								break;
							}
							case LookMode.TargetInfo:
							{
								TargetInfo value3 = (TargetInfo)(object)item8;
								Scribe_TargetInfo.Look(ref value3, saveDestroyedThings, "li");
								break;
							}
							case LookMode.GlobalTargetInfo:
							{
								GlobalTargetInfo value2 = (GlobalTargetInfo)(object)item8;
								Scribe_TargetInfo.Look(ref value2, saveDestroyedThings, "li");
								break;
							}
							case LookMode.Def:
							{
								Def value = (Def)(object)item8;
								Scribe_Defs.Look(ref value, "li");
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
								ILoadReferenceable refee = (ILoadReferenceable)(object)item8;
								Scribe_References.Look(ref refee, "li", saveDestroyedThings);
								break;
							}
							}
						}
					}
					else if (Scribe.mode == LoadSaveMode.LoadingVars)
					{
						XmlNode curXmlParent = Scribe.loader.curXmlParent;
						XmlAttribute xmlAttribute = curXmlParent.Attributes["IsNull"];
						if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
						{
							if (lookMode == LookMode.Reference)
							{
								Scribe.loader.crossRefs.loadIDs.RegisterLoadIDListReadFromXml(null, null);
							}
							list = null;
							return;
						}
						switch (lookMode)
						{
						case LookMode.Value:
							list = new List<T>(curXmlParent.ChildNodes.Count);
							foreach (XmlNode childNode in curXmlParent.ChildNodes)
							{
								T item = ScribeExtractor.ValueFromNode(childNode, default(T));
								list.Add(item);
							}
							break;
						case LookMode.Deep:
							list = new List<T>(curXmlParent.ChildNodes.Count);
							foreach (XmlNode childNode2 in curXmlParent.ChildNodes)
							{
								T item7 = ScribeExtractor.SaveableFromNode<T>(childNode2, ctorArgs);
								list.Add(item7);
							}
							break;
						case LookMode.Def:
							list = new List<T>(curXmlParent.ChildNodes.Count);
							foreach (XmlNode childNode3 in curXmlParent.ChildNodes)
							{
								T item6 = ScribeExtractor.DefFromNodeUnsafe<T>(childNode3);
								list.Add(item6);
							}
							break;
						case LookMode.BodyPart:
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num4 = 0;
							foreach (XmlNode childNode4 in curXmlParent.ChildNodes)
							{
								T item5 = (T)(object)ScribeExtractor.BodyPartFromNode(childNode4, num4.ToString(), null);
								list.Add(item5);
								num4++;
							}
							break;
						}
						case LookMode.LocalTargetInfo:
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num3 = 0;
							foreach (XmlNode childNode5 in curXmlParent.ChildNodes)
							{
								T item4 = (T)(object)ScribeExtractor.LocalTargetInfoFromNode(childNode5, num3.ToString(), LocalTargetInfo.Invalid);
								list.Add(item4);
								num3++;
							}
							break;
						}
						case LookMode.TargetInfo:
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num2 = 0;
							foreach (XmlNode childNode6 in curXmlParent.ChildNodes)
							{
								T item3 = (T)(object)ScribeExtractor.TargetInfoFromNode(childNode6, num2.ToString(), TargetInfo.Invalid);
								list.Add(item3);
								num2++;
							}
							break;
						}
						case LookMode.GlobalTargetInfo:
						{
							list = new List<T>(curXmlParent.ChildNodes.Count);
							int num = 0;
							foreach (XmlNode childNode7 in curXmlParent.ChildNodes)
							{
								T item2 = (T)(object)ScribeExtractor.GlobalTargetInfoFromNode(childNode7, num.ToString(), GlobalTargetInfo.Invalid);
								list.Add(item2);
								num++;
							}
							break;
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
					else
					{
						if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
						{
							return;
						}
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
						return;
					}
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
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

		public static void Look<K, V>(ref Dictionary<K, V> dict, string label, LookMode keyLookMode, LookMode valueLookMode, ref List<K> keysWorkingList, ref List<V> valuesWorkingList)
		{
			if (Scribe.EnterNode(label))
			{
				try
				{
					if (Scribe.mode == LoadSaveMode.Saving && dict == null)
					{
						Scribe.saver.WriteAttribute("IsNull", "True");
						return;
					}
					if (Scribe.mode == LoadSaveMode.LoadingVars)
					{
						XmlAttribute xmlAttribute = Scribe.loader.curXmlParent.Attributes["IsNull"];
						if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
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
						Look(ref keysWorkingList, "keys", keyLookMode);
						Look(ref valuesWorkingList, "values", valueLookMode);
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
					if (((flag && Scribe.mode == LoadSaveMode.ResolvingCrossRefs) || (!flag && Scribe.mode == LoadSaveMode.LoadingVars)) && dict != null)
					{
						if (keysWorkingList == null)
						{
							Log.Error("Cannot fill dictionary because there are no keys. label=" + label);
						}
						else if (valuesWorkingList == null)
						{
							Log.Error("Cannot fill dictionary because there are no values. label=" + label);
						}
						else
						{
							if (keysWorkingList.Count != valuesWorkingList.Count)
							{
								Log.Error("Keys count does not match the values count while loading a dictionary (maybe keys and values were resolved during different passes?). Some elements will be skipped. keys=" + keysWorkingList.Count + ", values=" + valuesWorkingList.Count + ", label=" + label);
							}
							int num = Math.Min(keysWorkingList.Count, valuesWorkingList.Count);
							for (int i = 0; i < num; i++)
							{
								if (keysWorkingList[i] == null)
								{
									Log.Error(string.Concat("Null key while loading dictionary of ", typeof(K), " and ", typeof(V), ". label=", label));
									continue;
								}
								try
								{
									dict.Add(keysWorkingList[i], valuesWorkingList[i]);
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
				finally
				{
					Scribe.ExitNode();
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars)
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
	}
}
