using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using JetBrains.Annotations;

namespace Verse;

public static class XmlToObjectUtils
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

		public override int GetHashCode()
		{
			return HashCode.Combine(type, fieldName);
		}
	}

	private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> fieldInfoLookup = new Dictionary<Type, Dictionary<string, FieldInfo>>();

	private static readonly Dictionary<FieldAliasCache, FieldInfo> fieldAliases = new Dictionary<FieldAliasCache, FieldInfo>(EqualityComparer<FieldAliasCache>.Default);

	private static readonly Dictionary<(Type, string), FieldInfo> getFieldCache = new Dictionary<(Type, string), FieldInfo>();

	private static readonly Dictionary<(Type, string), FieldInfo> getFieldIgnoreCaseCache = new Dictionary<(Type, string), FieldInfo>();

	private static Dictionary<Type, MethodInfo> customDataLoadMethodCache = new Dictionary<Type, MethodInfo>();

	public const string PostLoadMethodName = "PostLoad";

	private static Dictionary<Type, MethodInfo> postLoadMethodCache = new Dictionary<Type, MethodInfo>();

	[CanBeNull]
	public static FieldInfo DoFieldSearch(Type typeBeingDeserialized, XmlNode fieldNode, XmlNode xmlRootForDebug)
	{
		string name = fieldNode.Name;
		FieldInfo value = DirectGetFieldByName(typeBeingDeserialized, name, xmlRootForDebug);
		if (value != null)
		{
			return value;
		}
		DeepProfiler.Start("Search for field alias");
		try
		{
			FieldAliasCache key = new FieldAliasCache(typeBeingDeserialized, name);
			if (!fieldAliases.TryGetValue(key, out value))
			{
				FieldInfo[] fields = typeBeingDeserialized.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					foreach (LoadAliasAttribute customAttribute in fieldInfo.GetCustomAttributes<LoadAliasAttribute>(inherit: true))
					{
						if (customAttribute.alias.EqualsIgnoreCase(name))
						{
							value = fieldInfo;
							break;
						}
					}
					if (value != null)
					{
						break;
					}
				}
				fieldAliases.Add(key, value);
			}
		}
		finally
		{
			DeepProfiler.End();
		}
		if (value != null)
		{
			UnsavedAttribute unsavedAttribute = value.TryGetAttribute<UnsavedAttribute>();
			if (unsavedAttribute != null && !unsavedAttribute.allowLoading)
			{
				Log.Error("XML error: " + fieldNode.OuterXml + " corresponds to a field in type " + typeBeingDeserialized.Name + " which has an Unsaved attribute. Context: " + xmlRootForDebug.OuterXml);
				return null;
			}
		}
		if (value != null)
		{
			return value;
		}
		DeepProfiler.Start("Check for ignored missing field");
		try
		{
			bool flag = false;
			XmlAttribute xmlAttribute = fieldNode.Attributes?["IgnoreIfNoMatchingField"];
			if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
			{
				flag = true;
			}
			else
			{
				object[] customAttributes = typeBeingDeserialized.GetCustomAttributes(typeof(IgnoreSavedElementAttribute), inherit: true);
				for (int i = 0; i < customAttributes.Length; i++)
				{
					if (string.Equals(((IgnoreSavedElementAttribute)customAttributes[i]).elementToIgnore, name, StringComparison.OrdinalIgnoreCase))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				Log.Error("XML error: " + fieldNode.OuterXml + " doesn't correspond to any field in type " + typeBeingDeserialized.Name + ". Context: " + xmlRootForDebug.OuterXml);
			}
			return null;
		}
		finally
		{
			DeepProfiler.End();
		}
	}

	private static FieldInfo DirectGetFieldByName(Type type, string token, XmlNode debugXmlNode)
	{
		if (!fieldInfoLookup.TryGetValue(type, out var value))
		{
			value = new Dictionary<string, FieldInfo>();
			fieldInfoLookup.Add(type, value);
		}
		if (value.TryGetValue(token, out var value2))
		{
			return value2;
		}
		value2 = SearchTypeHierarchy(type, token, ignoreCase: false);
		if (value2 != null)
		{
			value.Add(token, value2);
			return value2;
		}
		value2 = SearchTypeHierarchy(type, token, ignoreCase: true);
		if (value2 == null)
		{
			return null;
		}
		value.Add(token, value2);
		if (type.HasAttribute<CaseInsensitiveXMLParsing>())
		{
			return value2;
		}
		string text = $"Attempt to use string {token} to refer to field {value2.Name} in type {type}; xml tags are now case-sensitive";
		if (debugXmlNode != null)
		{
			text = text + ". XML: " + debugXmlNode.OuterXml;
		}
		Log.Error(text);
		return value2;
	}

	private static FieldInfo SearchTypeHierarchy(Type type, string name, bool ignoreCase)
	{
		Dictionary<(Type, string), FieldInfo> dictionary = (ignoreCase ? getFieldIgnoreCaseCache : getFieldCache);
		BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		if (ignoreCase)
		{
			bindingFlags |= BindingFlags.IgnoreCase;
		}
		while (true)
		{
			if (dictionary.TryGetValue((type, name), out var value))
			{
				if (value != null)
				{
					return value;
				}
			}
			else
			{
				value = type.GetField(name, bindingFlags);
				dictionary.Add((type, name), value);
				if (value != null)
				{
					return value;
				}
			}
			Type baseType = type.BaseType;
			if ((object)baseType == null || !(baseType != typeof(object)))
			{
				break;
			}
			type = baseType;
		}
		return null;
	}

	public static MethodInfo CustomDataLoadMethodOf(Type type)
	{
		if (customDataLoadMethodCache.TryGetValue(type, out var value))
		{
			return value;
		}
		MethodInfo method = type.GetMethod("LoadDataFromXmlCustom", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		customDataLoadMethodCache.Add(type, method);
		return method;
	}

	public static MethodInfo PostLoadMethodOf(Type type)
	{
		if (postLoadMethodCache.TryGetValue(type, out var value))
		{
			return value;
		}
		MethodInfo method = type.GetMethod("PostLoad");
		postLoadMethodCache.Add(type, method);
		return method;
	}
}
