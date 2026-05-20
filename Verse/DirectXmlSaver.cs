using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Verse;

public static class DirectXmlSaver
{
	public static bool IsSimpleTextType(Type type)
	{
		if (!(type == typeof(float)) && !(type == typeof(double)) && !(type == typeof(long)) && !(type == typeof(ulong)) && !(type == typeof(char)) && !(type == typeof(byte)) && !(type == typeof(sbyte)) && !(type == typeof(int)) && !(type == typeof(uint)) && !(type == typeof(bool)) && !(type == typeof(short)) && !(type == typeof(ushort)) && !(type == typeof(string)))
		{
			return type.IsEnum;
		}
		return true;
	}

	public static void SaveDataObject(object obj, string filePath)
	{
		try
		{
			XDocument xDocument = new XDocument();
			XElement content = XElementFromObject(obj, obj.GetType());
			xDocument.Add(content);
			xDocument.Save(filePath);
		}
		catch (Exception ex)
		{
			Log.Error("Exception saving data object " + obj.ToStringSafe() + ": " + ex);
			GenUI.ErrorDialog("ProblemSavingFile".Translate(filePath, ex.ToString()));
		}
	}

	public static XElement XElementFromObject(object obj, Type expectedClass)
	{
		return XElementFromObject(obj, expectedClass, expectedClass.Name);
	}

	public static XElement XElementFromObject(object obj, Type expectedType, string nodeName, FieldInfo owningField = null, bool saveDefsAsRefs = false)
	{
		if (owningField != null && owningField.TryGetAttribute<DefaultValueAttribute>(out var customAttribute) && customAttribute.ObjIsDefault(obj))
		{
			return null;
		}
		if (obj == null)
		{
			XElement xElement = new XElement(nodeName);
			xElement.SetAttributeValue("IsNull", "True");
			return xElement;
		}
		Type type = obj.GetType();
		XElement xElement2 = new XElement(nodeName);
		if (IsSimpleTextType(type))
		{
			xElement2.Add(new XText(obj.ToString()));
		}
		else if (saveDefsAsRefs && GenTypes.IsDef(type))
		{
			string defName = ((Def)obj).defName;
			xElement2.Add(new XText(defName));
		}
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			Type expectedType2 = type.GetGenericArguments()[0];
			int num = (int)type.GetProperty("Count").GetValue(obj, null);
			for (int i = 0; i < num; i++)
			{
				object[] index = new object[1] { i };
				XNode content = XElementFromObject(type.GetProperty("Item").GetValue(obj, index), expectedType2, "li", null, saveDefsAsRefs: true);
				xElement2.Add(content);
			}
		}
		else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
		{
			Type expectedType3 = type.GetGenericArguments()[0];
			Type expectedType4 = type.GetGenericArguments()[1];
			foreach (object item in obj as IEnumerable)
			{
				object value = item.GetType().GetProperty("Key").GetValue(item, null);
				object value2 = item.GetType().GetProperty("Value").GetValue(item, null);
				XElement xElement3 = new XElement("li");
				xElement3.Add(XElementFromObject(value, expectedType3, "key", null, saveDefsAsRefs: true));
				xElement3.Add(XElementFromObject(value2, expectedType4, "value", null, saveDefsAsRefs: true));
				xElement2.Add(xElement3);
			}
		}
		else
		{
			if (type != expectedType)
			{
				XAttribute content2 = new XAttribute("Class", GenTypes.GetTypeNameWithoutIgnoredNamespaces(obj.GetType()));
				xElement2.Add(content2);
			}
			foreach (FieldInfo item2 in from f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				orderby f.MetadataToken
				select f)
			{
				try
				{
					XElement xElement4 = XElementFromField(item2, obj);
					if (xElement4 != null)
					{
						xElement2.Add(xElement4);
					}
				}
				catch
				{
					throw;
				}
			}
		}
		return xElement2;
	}

	private static XElement XElementFromField(FieldInfo fi, object owningObj)
	{
		if (Attribute.IsDefined(fi, typeof(UnsavedAttribute)))
		{
			return null;
		}
		return XElementFromObject(fi.GetValue(owningObj), fi.FieldType, fi.Name, fi);
	}
}
