using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Verse
{
	public static class DirectXmlToObject
	{
		private struct FieldAliasCache : IEquatable<FieldAliasCache>
		{
			public Type type;

			public string fieldName;

			public FieldAliasCache(Type type, string fieldName)
			{
				this.type = type;
				this.fieldName = fieldName.ToLower();
			}

			public bool Equals(FieldAliasCache other)
			{
				if (type == other.type)
				{
					return string.Equals(fieldName, other.fieldName);
				}
				return false;
			}
		}

		public static Stack<Type> currentlyInstantiatingObjectOfType = new Stack<Type>();

		public const string DictionaryKeyName = "key";

		public const string DictionaryValueName = "value";

		public const string LoadDataFromXmlCustomMethodName = "LoadDataFromXmlCustom";

		public const string PostLoadMethodName = "PostLoad";

		public const string ObjectFromXmlMethodName = "ObjectFromXmlReflection";

		public const string ListFromXmlMethodName = "ListFromXmlReflection";

		public const string DictionaryFromXmlMethodName = "DictionaryFromXmlReflection";

		private static Dictionary<Type, Func<XmlNode, object>> listFromXmlMethods = new Dictionary<Type, Func<XmlNode, object>>();

		private static Dictionary<Type, Func<XmlNode, object>> dictionaryFromXmlMethods = new Dictionary<Type, Func<XmlNode, object>>();

		private static readonly Type[] tmpOneTypeArray = new Type[1];

		private static readonly Dictionary<Type, Func<XmlNode, bool, object>> objectFromXmlMethods = new Dictionary<Type, Func<XmlNode, bool, object>>();

		private static Dictionary<FieldAliasCache, FieldInfo> fieldAliases = new Dictionary<FieldAliasCache, FieldInfo>(EqualityComparer<FieldAliasCache>.Default);

		private static Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoLookup = new Dictionary<Type, Dictionary<string, FieldInfo>>();

		public static Func<XmlNode, bool, object> GetObjectFromXmlMethod(Type type)
		{
			if (!objectFromXmlMethods.TryGetValue(type, out Func<XmlNode, bool, object> value))
			{
				MethodInfo method = typeof(DirectXmlToObject).GetMethod("ObjectFromXmlReflection", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				tmpOneTypeArray[0] = type;
				value = (Func<XmlNode, bool, object>)Delegate.CreateDelegate(typeof(Func<XmlNode, bool, object>), method.MakeGenericMethod(tmpOneTypeArray));
				objectFromXmlMethods.Add(type, value);
			}
			return value;
		}

		private static object ObjectFromXmlReflection<T>(XmlNode xmlRoot, bool doPostLoad)
		{
			return ObjectFromXml<T>(xmlRoot, doPostLoad);
		}

		public static T ObjectFromXml<T>(XmlNode xmlRoot, bool doPostLoad)
		{
			MethodInfo methodInfo = CustomDataLoadMethodOf(typeof(T));
			if (methodInfo != null)
			{
				xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
				Type type = ClassTypeOf<T>(xmlRoot);
				currentlyInstantiatingObjectOfType.Push(type);
				T val;
				try
				{
					val = (T)Activator.CreateInstance(type);
				}
				finally
				{
					currentlyInstantiatingObjectOfType.Pop();
				}
				try
				{
					methodInfo.Invoke(val, new object[1]
					{
						xmlRoot
					});
				}
				catch (Exception ex)
				{
					Log.Error(string.Concat("Exception in custom XML loader for ", typeof(T), ". Node is:\n ", xmlRoot.OuterXml, "\n\nException is:\n ", ex.ToString()));
					val = default(T);
				}
				if (doPostLoad)
				{
					TryDoPostLoad(val);
				}
				return val;
			}
			if (typeof(ISlateRef).IsAssignableFrom(typeof(T)))
			{
				try
				{
					return ParseHelper.FromString<T>(InnerTextWithReplacedNewlinesOrXML(xmlRoot));
				}
				catch (Exception ex2)
				{
					Log.Error(string.Concat("Exception parsing ", xmlRoot.OuterXml, " to type ", typeof(T), ": ", ex2));
				}
				return default(T);
			}
			if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.CDATA)
			{
				if (typeof(T) != typeof(string))
				{
					Log.Error("CDATA can only be used for strings. Bad xml: " + xmlRoot.OuterXml);
					return default(T);
				}
				return (T)(object)xmlRoot.FirstChild.Value;
			}
			if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.Text)
			{
				try
				{
					return ParseHelper.FromString<T>(xmlRoot.InnerText);
				}
				catch (Exception ex3)
				{
					Log.Error(string.Concat("Exception parsing ", xmlRoot.OuterXml, " to type ", typeof(T), ": ", ex3));
				}
				return default(T);
			}
			if (Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
			{
				List<T> list = ListFromXml<T>(xmlRoot);
				int num = 0;
				foreach (T item in list)
				{
					int num2 = (int)(object)item;
					num |= num2;
				}
				return (T)(object)num;
			}
			if (typeof(T).HasGenericDefinition(typeof(List<>)))
			{
				Func<XmlNode, object> value = null;
				if (!listFromXmlMethods.TryGetValue(typeof(T), out value))
				{
					MethodInfo method = typeof(DirectXmlToObject).GetMethod("ListFromXmlReflection", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					Type[] genericArguments = typeof(T).GetGenericArguments();
					value = (Func<XmlNode, object>)Delegate.CreateDelegate(typeof(Func<XmlNode, object>), method.MakeGenericMethod(genericArguments));
					listFromXmlMethods.Add(typeof(T), value);
				}
				return (T)value(xmlRoot);
			}
			if (typeof(T).HasGenericDefinition(typeof(Dictionary<, >)))
			{
				Func<XmlNode, object> value2 = null;
				if (!dictionaryFromXmlMethods.TryGetValue(typeof(T), out value2))
				{
					MethodInfo method2 = typeof(DirectXmlToObject).GetMethod("DictionaryFromXmlReflection", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					Type[] genericArguments2 = typeof(T).GetGenericArguments();
					value2 = (Func<XmlNode, object>)Delegate.CreateDelegate(typeof(Func<XmlNode, object>), method2.MakeGenericMethod(genericArguments2));
					dictionaryFromXmlMethods.Add(typeof(T), value2);
				}
				return (T)value2(xmlRoot);
			}
			if (!xmlRoot.HasChildNodes)
			{
				if (typeof(T) == typeof(string))
				{
					return (T)(object)"";
				}
				XmlAttribute xmlAttribute = xmlRoot.Attributes["IsNull"];
				if (xmlAttribute != null && xmlAttribute.Value.ToUpperInvariant() == "TRUE")
				{
					return default(T);
				}
				if (typeof(T).IsGenericType)
				{
					Type genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
					if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(HashSet<>) || genericTypeDefinition == typeof(Dictionary<, >))
					{
						return Activator.CreateInstance<T>();
					}
				}
			}
			xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
			Type type2 = ClassTypeOf<T>(xmlRoot);
			Type type3 = Nullable.GetUnderlyingType(type2) ?? type2;
			currentlyInstantiatingObjectOfType.Push(type3);
			T val2;
			try
			{
				val2 = (T)Activator.CreateInstance(type3);
			}
			finally
			{
				currentlyInstantiatingObjectOfType.Pop();
			}
			HashSet<string> hashSet = null;
			if (xmlRoot.ChildNodes.Count > 1)
			{
				hashSet = new HashSet<string>();
			}
			for (int i = 0; i < xmlRoot.ChildNodes.Count; i++)
			{
				XmlNode xmlNode = xmlRoot.ChildNodes[i];
				if (xmlNode is XmlComment)
				{
					continue;
				}
				if (xmlRoot.ChildNodes.Count > 1)
				{
					if (hashSet.Contains(xmlNode.Name))
					{
						Log.Error(string.Concat("XML ", typeof(T), " defines the same field twice: ", xmlNode.Name, ".\n\nField contents: ", xmlNode.InnerText, ".\n\nWhole XML:\n\n", xmlRoot.OuterXml));
					}
					else
					{
						hashSet.Add(xmlNode.Name);
					}
				}
				FieldInfo value3 = null;
				DeepProfiler.Start("GetFieldInfoForType");
				try
				{
					value3 = GetFieldInfoForType(val2.GetType(), xmlNode.Name, xmlRoot);
				}
				finally
				{
					DeepProfiler.End();
				}
				if (value3 == null)
				{
					DeepProfiler.Start("Field search");
					try
					{
						FieldAliasCache key = new FieldAliasCache(val2.GetType(), xmlNode.Name);
						if (!fieldAliases.TryGetValue(key, out value3))
						{
							FieldInfo[] fields = val2.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							foreach (FieldInfo fieldInfo in fields)
							{
								object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(LoadAliasAttribute), inherit: true);
								for (int k = 0; k < customAttributes.Length; k++)
								{
									if (((LoadAliasAttribute)customAttributes[k]).alias.EqualsIgnoreCase(xmlNode.Name))
									{
										value3 = fieldInfo;
										break;
									}
								}
								if (value3 != null)
								{
									break;
								}
							}
							fieldAliases.Add(key, value3);
						}
					}
					finally
					{
						DeepProfiler.End();
					}
				}
				if (value3 != null && value3.TryGetAttribute<UnsavedAttribute>() != null && !value3.TryGetAttribute<UnsavedAttribute>().allowLoading)
				{
					Log.Error("XML error: " + xmlNode.OuterXml + " corresponds to a field in type " + val2.GetType().Name + " which has an Unsaved attribute. Context: " + xmlRoot.OuterXml);
				}
				else if (value3 == null)
				{
					DeepProfiler.Start("Field search 2");
					try
					{
						bool flag = false;
						XmlAttribute xmlAttribute2 = xmlNode.Attributes?["IgnoreIfNoMatchingField"];
						if (xmlAttribute2 != null && xmlAttribute2.Value.ToUpperInvariant() == "TRUE")
						{
							flag = true;
						}
						else
						{
							object[] customAttributes = val2.GetType().GetCustomAttributes(typeof(IgnoreSavedElementAttribute), inherit: true);
							for (int j = 0; j < customAttributes.Length; j++)
							{
								if (string.Equals(((IgnoreSavedElementAttribute)customAttributes[j]).elementToIgnore, xmlNode.Name, StringComparison.OrdinalIgnoreCase))
								{
									flag = true;
									break;
								}
							}
						}
						if (!flag)
						{
							Log.Error("XML error: " + xmlNode.OuterXml + " doesn't correspond to any field in type " + val2.GetType().Name + ". Context: " + xmlRoot.OuterXml);
						}
					}
					finally
					{
						DeepProfiler.End();
					}
				}
				else if (typeof(Def).IsAssignableFrom(value3.FieldType))
				{
					if (xmlNode.InnerText.NullOrEmpty())
					{
						value3.SetValue(val2, null);
						continue;
					}
					XmlAttribute xmlAttribute3 = xmlNode.Attributes["MayRequire"];
					DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(val2, value3, xmlNode.InnerText, xmlAttribute3?.Value.ToLower());
				}
				else
				{
					object obj = null;
					try
					{
						obj = GetObjectFromXmlMethod(value3.FieldType)(xmlNode, doPostLoad);
					}
					catch (Exception ex4)
					{
						Log.Error("Exception loading from " + xmlNode.ToString() + ": " + ex4.ToString());
						continue;
					}
					if (!typeof(T).IsValueType)
					{
						value3.SetValue(val2, obj);
						continue;
					}
					object obj2 = val2;
					value3.SetValue(obj2, obj);
					val2 = (T)obj2;
				}
			}
			if (doPostLoad)
			{
				TryDoPostLoad(val2);
			}
			return val2;
		}

		private static Type ClassTypeOf<T>(XmlNode xmlRoot)
		{
			XmlAttribute xmlAttribute = xmlRoot.Attributes["Class"];
			if (xmlAttribute != null)
			{
				Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(xmlAttribute.Value, typeof(T).Namespace);
				if (typeInAnyAssembly == null)
				{
					Log.Error("Could not find type named " + xmlAttribute.Value + " from node " + xmlRoot.OuterXml);
					return typeof(T);
				}
				return typeInAnyAssembly;
			}
			return typeof(T);
		}

		private static void TryDoPostLoad(object obj)
		{
			DeepProfiler.Start("TryDoPostLoad");
			try
			{
				MethodInfo method = obj.GetType().GetMethod("PostLoad");
				if (method != null)
				{
					method.Invoke(obj, null);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Exception while executing PostLoad on " + obj.ToStringSafe() + ": " + ex);
			}
			finally
			{
				DeepProfiler.End();
			}
		}

		private static object ListFromXmlReflection<T>(XmlNode listRootNode)
		{
			return ListFromXml<T>(listRootNode);
		}

		private static List<T> ListFromXml<T>(XmlNode listRootNode)
		{
			List<T> list = new List<T>();
			try
			{
				bool flag = typeof(Def).IsAssignableFrom(typeof(T));
				foreach (XmlNode childNode in listRootNode.ChildNodes)
				{
					if (!ValidateListNode(childNode, listRootNode, typeof(T)))
					{
						continue;
					}
					XmlAttribute xmlAttribute = childNode.Attributes["MayRequire"];
					if (flag)
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef(list, childNode.InnerText, listRootNode.Name, xmlAttribute?.Value);
						continue;
					}
					try
					{
						if (xmlAttribute == null || xmlAttribute.Value.NullOrEmpty() || ModsConfig.IsActive(xmlAttribute.Value))
						{
							list.Add(ObjectFromXml<T>(childNode, doPostLoad: true));
						}
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat("Exception loading list element from XML: ", ex, "\nXML:\n", listRootNode.OuterXml));
					}
				}
				return list;
			}
			catch (Exception ex2)
			{
				Log.Error(string.Concat("Exception loading list from XML: ", ex2, "\nXML:\n", listRootNode.OuterXml));
				return list;
			}
		}

		private static object DictionaryFromXmlReflection<K, V>(XmlNode dictRootNode)
		{
			return DictionaryFromXml<K, V>(dictRootNode);
		}

		private static Dictionary<K, V> DictionaryFromXml<K, V>(XmlNode dictRootNode)
		{
			Dictionary<K, V> dictionary = new Dictionary<K, V>();
			try
			{
				bool num = typeof(Def).IsAssignableFrom(typeof(K));
				bool flag = typeof(Def).IsAssignableFrom(typeof(V));
				if (!num && !flag)
				{
					foreach (XmlNode childNode in dictRootNode.ChildNodes)
					{
						if (ValidateListNode(childNode, dictRootNode, typeof(KeyValuePair<K, V>)))
						{
							K key = ObjectFromXml<K>(childNode["key"], doPostLoad: true);
							V value = ObjectFromXml<V>(childNode["value"], doPostLoad: true);
							dictionary.Add(key, value);
						}
					}
					return dictionary;
				}
				foreach (XmlNode childNode2 in dictRootNode.ChildNodes)
				{
					if (ValidateListNode(childNode2, dictRootNode, typeof(KeyValuePair<K, V>)))
					{
						DirectXmlCrossRefLoader.RegisterDictionaryWantsCrossRef(dictionary, childNode2, dictRootNode.Name);
					}
				}
				return dictionary;
			}
			catch (Exception ex)
			{
				Log.Error("Malformed dictionary XML. Node: " + dictRootNode.OuterXml + ".\n\nException: " + ex);
				return dictionary;
			}
		}

		private static MethodInfo CustomDataLoadMethodOf(Type type)
		{
			return type.GetMethod("LoadDataFromXmlCustom", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}

		private static bool ValidateListNode(XmlNode listEntryNode, XmlNode listRootNode, Type listItemType)
		{
			if (listEntryNode is XmlComment)
			{
				return false;
			}
			if (listEntryNode is XmlText)
			{
				Log.Error("XML format error: Raw text found inside a list element. Did you mean to surround it with list item <li> tags? " + listRootNode.OuterXml);
				return false;
			}
			if (listEntryNode.Name != "li" && CustomDataLoadMethodOf(listItemType) == null)
			{
				Log.Error("XML format error: List item found with name that is not <li>, and which does not have a custom XML loader method, in " + listRootNode.OuterXml);
				return false;
			}
			return true;
		}

		private static FieldInfo GetFieldInfoForType(Type type, string token, XmlNode debugXmlNode)
		{
			Dictionary<string, FieldInfo> dictionary = fieldInfoLookup.TryGetValue(type);
			if (dictionary == null)
			{
				dictionary = new Dictionary<string, FieldInfo>();
				fieldInfoLookup[type] = dictionary;
			}
			FieldInfo fieldInfo = dictionary.TryGetValue(token);
			if (fieldInfo == null && !dictionary.ContainsKey(token))
			{
				fieldInfo = SearchTypeHierarchy(type, token, BindingFlags.Default);
				if (fieldInfo == null)
				{
					fieldInfo = SearchTypeHierarchy(type, token, BindingFlags.IgnoreCase);
					if (fieldInfo != null && !type.HasAttribute<CaseInsensitiveXMLParsing>())
					{
						string text = $"Attempt to use string {token} to refer to field {fieldInfo.Name} in type {type}; xml tags are now case-sensitive";
						if (debugXmlNode != null)
						{
							text = text + ". XML: " + debugXmlNode.OuterXml;
						}
						Log.Error(text);
					}
				}
				dictionary[token] = fieldInfo;
			}
			return fieldInfo;
		}

		private static FieldInfo SearchTypeHierarchy(Type type, string token, BindingFlags extraFlags)
		{
			FieldInfo fieldInfo = null;
			while (true)
			{
				fieldInfo = type.GetField(token, extraFlags | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (!(fieldInfo == null) || !(type.BaseType != typeof(object)))
				{
					break;
				}
				type = type.BaseType;
			}
			return fieldInfo;
		}

		public static string InnerTextWithReplacedNewlinesOrXML(XmlNode xmlNode)
		{
			if (xmlNode.ChildNodes.Count == 1 && xmlNode.FirstChild.NodeType == XmlNodeType.Text)
			{
				return xmlNode.InnerText.Replace("\\n", "\n");
			}
			return xmlNode.InnerXml;
		}
	}
}
