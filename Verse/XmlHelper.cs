using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Verse;

public static class XmlHelper
{
	private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public static bool TryParseSingleDefault(object requester, XmlNode root, string fieldName)
	{
		if (root.ChildNodes.Count != 1 || root.FirstChild.Isnt<XmlText>(out var casted) || fieldName.NullOrEmpty() || fieldName == "li")
		{
			return false;
		}
		FieldInfo field = requester.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field == null)
		{
			Log.Error("Tried to get default field which does not exist: " + fieldName + " on " + requester.GetType().Name);
			return false;
		}
		if (typeof(Def).IsAssignableFrom(field.FieldType))
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(requester, field.Name, casted.InnerText);
		}
		else
		{
			field.SetValue(requester, ParseHelper.FromString(casted.InnerText, field.FieldType));
		}
		return true;
	}

	public static void ParseElements(object requester, XmlNode root, string rootField, string defaultField = null)
	{
		if (root.Name != "li")
		{
			FieldInfo field = requester.GetType().GetField(rootField, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				Log.Error("Tried to get root field which does not exist: " + rootField + " on " + requester.GetType().Name);
				return;
			}
			if (typeof(Def).IsAssignableFrom(field.FieldType))
			{
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(requester, field.Name, root.Name);
			}
			else
			{
				field.SetValue(requester, ParseHelper.FromString(root.Name, field.FieldType));
			}
			if (TryParseSingleDefault(requester, root, defaultField))
			{
				return;
			}
			{
				foreach (object childNode in root.ChildNodes)
				{
					if (!(childNode is XmlElement xmlElement))
					{
						continue;
					}
					FieldInfo field2 = requester.GetType().GetField(xmlElement.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (field2 == null)
					{
						Log.Error("Tried to set a value (" + xmlElement.InnerText + ") to a field which does not exist: " + xmlElement.Name + " on " + requester.GetType().Name);
					}
					else if (typeof(Def).IsAssignableFrom(field2.FieldType))
					{
						DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(requester, field2.Name, xmlElement.InnerText);
					}
					else if (typeof(IList).IsAssignableFrom(field2.FieldType))
					{
						IList list = (IList)Activator.CreateInstance(field2.FieldType);
						Type type = list.GetType().GenericTypeArguments[0];
						field2.SetValue(requester, list);
						if (typeof(Def).IsAssignableFrom(type))
						{
							Log.Error("Cannot use lists of defs in XML Helper Parse Elements");
							continue;
						}
						foreach (object childNode2 in xmlElement.ChildNodes)
						{
							if (childNode2 is XmlElement xmlElement2)
							{
								list.Add(ParseHelper.FromString(xmlElement2.InnerText, type));
							}
						}
					}
					else
					{
						field2.SetValue(requester, ParseHelper.FromString(xmlElement.InnerText, field2.FieldType));
					}
				}
				return;
			}
		}
		FieldInfo field3 = requester.GetType().GetField(rootField, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field3 == null)
		{
			Log.Error("Tried to get root field which does not exist: " + rootField + " on " + requester.GetType().Name);
		}
		else if (typeof(Def).IsAssignableFrom(field3.FieldType))
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(requester, field3.Name, root.InnerText);
		}
		else
		{
			field3.SetValue(requester, ParseHelper.FromString(root.InnerText, field3.FieldType));
		}
	}
}
