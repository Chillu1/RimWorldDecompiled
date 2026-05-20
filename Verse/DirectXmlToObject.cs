using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Verse;

public static class DirectXmlToObject
{
	public static Stack<Type> currentlyInstantiatingObjectOfType = new Stack<Type>();

	public const string DictionaryKeyName = "key";

	public const string DictionaryValueName = "value";

	public const string LoadDataFromXmlCustomMethodName = "LoadDataFromXmlCustom";

	private static Dictionary<Type, Func<XmlNode, object>> listFromXmlMethods = new Dictionary<Type, Func<XmlNode, object>>();

	private static Dictionary<Type, Func<XmlNode, object>> dictionaryFromXmlMethods = new Dictionary<Type, Func<XmlNode, object>>();

	private static readonly Type[] tmpOneTypeArray = new Type[1];

	private static readonly Dictionary<Type, Func<XmlNode, bool, object>> objectFromXmlMethods = new Dictionary<Type, Func<XmlNode, bool, object>>();

	public static Func<XmlNode, bool, object> GetObjectFromXmlMethod(Type type)
	{
		if (!objectFromXmlMethods.TryGetValue(type, out var value))
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
		XmlAttribute xmlAttribute = xmlRoot.Attributes["IsNull"];
		if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
		{
			return default(T);
		}
		Type typeFromHandle = typeof(T);
		MethodInfo methodInfo = XmlToObjectUtils.CustomDataLoadMethodOf(typeFromHandle);
		T val;
		Type type;
		if (methodInfo != null)
		{
			xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
			type = ClassTypeOf<T>(xmlRoot);
			currentlyInstantiatingObjectOfType.Push(type);
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
				methodInfo.Invoke(val, new object[1] { xmlRoot });
			}
			catch (Exception ex)
			{
				Log.Error("Exception in custom XML loader for " + typeFromHandle?.ToString() + ". Node is:\n " + xmlRoot.OuterXml + "\n\nException is:\n " + ex);
				val = default(T);
			}
			if (doPostLoad)
			{
				TryDoPostLoad(val);
			}
			return val;
		}
		if (GenTypes.IsSlateRef(typeFromHandle))
		{
			try
			{
				return ParseHelper.FromString<T>(InnerTextWithReplacedNewlinesOrXML(xmlRoot));
			}
			catch (Exception ex2)
			{
				Log.Error("Exception parsing " + xmlRoot.OuterXml + " to type " + typeFromHandle?.ToString() + ": " + ex2);
			}
			return default(T);
		}
		if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.Text)
		{
			try
			{
				return ParseHelper.FromString<T>(xmlRoot.InnerText);
			}
			catch (Exception ex3)
			{
				Log.Error("Exception parsing " + xmlRoot.OuterXml + " to type " + typeFromHandle?.ToString() + ": " + ex3);
			}
			return default(T);
		}
		if (xmlRoot.ChildNodes.Count == 1 && xmlRoot.FirstChild.NodeType == XmlNodeType.CDATA)
		{
			if (typeFromHandle != typeof(string))
			{
				Log.Error("CDATA can only be used for strings. Bad xml: " + xmlRoot.OuterXml);
				return default(T);
			}
			return (T)(object)xmlRoot.FirstChild.Value;
		}
		if (GenTypes.HasFlagsAttribute(typeFromHandle))
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
		if (GenTypes.IsList(typeFromHandle))
		{
			Func<XmlNode, object> value = null;
			if (!listFromXmlMethods.TryGetValue(typeFromHandle, out value))
			{
				MethodInfo method = typeof(DirectXmlToObject).GetMethod("ListFromXmlReflection", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				Type[] genericArguments = typeFromHandle.GetGenericArguments();
				value = (Func<XmlNode, object>)Delegate.CreateDelegate(typeof(Func<XmlNode, object>), method.MakeGenericMethod(genericArguments));
				listFromXmlMethods.Add(typeFromHandle, value);
			}
			return (T)value(xmlRoot);
		}
		if (GenTypes.IsDictionary(typeFromHandle))
		{
			Func<XmlNode, object> value2 = null;
			if (!dictionaryFromXmlMethods.TryGetValue(typeFromHandle, out value2))
			{
				MethodInfo method2 = typeof(DirectXmlToObject).GetMethod("DictionaryFromXmlReflection", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				Type[] genericArguments2 = typeFromHandle.GetGenericArguments();
				value2 = (Func<XmlNode, object>)Delegate.CreateDelegate(typeof(Func<XmlNode, object>), method2.MakeGenericMethod(genericArguments2));
				dictionaryFromXmlMethods.Add(typeFromHandle, value2);
			}
			return (T)value2(xmlRoot);
		}
		if (!xmlRoot.HasChildNodes)
		{
			if (typeFromHandle == typeof(string))
			{
				return (T)(object)"";
			}
			XmlAttribute xmlAttribute2 = xmlRoot.Attributes["IsNull"];
			if (xmlAttribute2 != null && xmlAttribute2.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
			{
				return default(T);
			}
			if (GenTypes.IsListHashSetOrDictionary(typeFromHandle))
			{
				return (T)Activator.CreateInstance(typeof(T));
			}
		}
		xmlRoot = XmlInheritance.GetResolvedNodeFor(xmlRoot);
		type = ClassTypeOf<T>(xmlRoot);
		Type type2 = Nullable.GetUnderlyingType(type) ?? type;
		currentlyInstantiatingObjectOfType.Push(type2);
		try
		{
			val = (T)Activator.CreateInstance(type2);
		}
		catch (InvalidCastException)
		{
			throw new InvalidCastException($"Cannot cast XML type {type2} to C# type {typeof(T)}.");
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
		XmlNodeList childNodes = xmlRoot.ChildNodes;
		int count = childNodes.Count;
		for (int i = 0; i < count; i++)
		{
			XmlNode xmlNode = childNodes[i];
			if (xmlNode is XmlComment)
			{
				continue;
			}
			if (count > 1 && !hashSet.Add(xmlNode.Name))
			{
				Log.Error("XML " + typeFromHandle?.ToString() + " defines the same field twice: " + xmlNode.Name + ".\n\nField contents: " + xmlNode.InnerText + ".\n\nWhole XML:\n\n" + xmlRoot.OuterXml);
			}
			FieldInfo fieldInfo = XmlToObjectUtils.DoFieldSearch(type2, xmlNode, xmlRoot);
			if (fieldInfo == null)
			{
				continue;
			}
			if (GenTypes.IsDef(fieldInfo.FieldType))
			{
				if (xmlNode.InnerText.NullOrEmpty())
				{
					fieldInfo.SetValue(val, null);
					continue;
				}
				XmlAttribute xmlAttribute3 = xmlNode.Attributes["MayRequire"];
				XmlAttribute xmlAttribute4 = xmlNode.Attributes["MayRequireAnyOf"];
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(val, fieldInfo, xmlNode.InnerText, xmlAttribute3?.Value.ToLower(), xmlAttribute4?.Value.ToLower());
				continue;
			}
			object obj = null;
			try
			{
				obj = GetObjectFromXmlMethod(fieldInfo.FieldType)(xmlNode, doPostLoad);
			}
			catch (Exception ex5)
			{
				Log.Error("Exception loading from " + xmlNode?.ToString() + ": " + ex5);
				continue;
			}
			if (!typeFromHandle.IsValueType)
			{
				fieldInfo.SetValue(val, obj);
				continue;
			}
			object obj2 = val;
			fieldInfo.SetValue(obj2, obj);
			val = (T)obj2;
		}
		if (doPostLoad)
		{
			TryDoPostLoad(val);
		}
		return val;
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
			MethodInfo methodInfo = XmlToObjectUtils.PostLoadMethodOf(obj.GetType());
			if (methodInfo != null)
			{
				methodInfo.Invoke(obj, null);
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
			bool flag = GenTypes.IsDef(typeof(T));
			foreach (XmlNode childNode in listRootNode.ChildNodes)
			{
				if (!ValidateListNode(childNode, listRootNode, typeof(T)))
				{
					continue;
				}
				XmlAttribute xmlAttribute = childNode.Attributes["MayRequire"];
				XmlAttribute xmlAttribute2 = childNode.Attributes["MayRequireAnyOf"];
				if (flag)
				{
					DirectXmlCrossRefLoader.RegisterListWantsCrossRef(list, childNode.InnerText, listRootNode.Name, xmlAttribute?.Value, xmlAttribute2?.Value);
				}
				else if (xmlAttribute != null && !xmlAttribute.Value.NullOrEmpty() && !ModLister.AllModsActiveNoSuffix(xmlAttribute.Value.Split(',')))
				{
					if (DirectXmlCrossRefLoader.MistypedMayRequire(xmlAttribute.Value))
					{
						Log.Error("Faulty MayRequire: " + xmlAttribute.Value);
					}
				}
				else if (xmlAttribute2 == null || xmlAttribute2.Value.NullOrEmpty() || ModLister.AnyModActiveNoSuffix(xmlAttribute2.Value.Split(',')))
				{
					try
					{
						list.Add(ObjectFromXml<T>(childNode, doPostLoad: true));
					}
					catch (Exception arg)
					{
						Log.Error($"Exception loading list element {typeof(T)} from XML: {arg}\nXML:\n{listRootNode.OuterXml}");
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Exception loading list from XML: " + ex?.ToString() + "\nXML:\n" + listRootNode.OuterXml);
		}
		return list;
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
			bool num = GenTypes.IsDef(typeof(K));
			bool flag = GenTypes.IsDef(typeof(V));
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
			}
			else
			{
				foreach (XmlNode childNode2 in dictRootNode.ChildNodes)
				{
					if (ValidateListNode(childNode2, dictRootNode, typeof(KeyValuePair<K, V>)))
					{
						DirectXmlCrossRefLoader.RegisterDictionaryWantsCrossRef(dictionary, childNode2, dictRootNode.Name);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Malformed dictionary XML. Node: " + dictRootNode.OuterXml + ".\n\nException: " + ex);
		}
		return dictionary;
	}

	public static bool ValidateListNode(XmlNode listEntryNode, XmlNode listRootNode, Type listItemType)
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
		if (listEntryNode.Name != "li" && XmlToObjectUtils.CustomDataLoadMethodOf(listItemType) == null)
		{
			Log.Error("XML format error: List item found with name " + listEntryNode.Name + " that is not <li>, and which does not have a custom XML loader method, in " + listRootNode.OuterXml);
			return false;
		}
		return true;
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
