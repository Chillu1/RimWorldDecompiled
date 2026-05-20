using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Verse;

public static class DirectXmlSaveLoadUtility
{
	public const BindingFlags FieldGetFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public static string GetXPath(this XmlNode node)
	{
		string text = "";
		while (node != null)
		{
			switch (node.NodeType)
			{
			case XmlNodeType.Element:
			{
				bool multiple;
				int elementIndexForXPath = GetElementIndexForXPath((XmlElement)node, out multiple);
				text = ((!multiple && !(node.Name == "li")) ? ("/" + node.Name + text) : ("/" + node.Name + "[" + elementIndexForXPath + "]" + text));
				node = node.ParentNode;
				break;
			}
			case XmlNodeType.Attribute:
				text = "/@" + node.Name + text;
				node = ((XmlAttribute)node).OwnerElement;
				break;
			case XmlNodeType.Document:
				return text;
			default:
				text = "/?" + text;
				node = node.ParentNode;
				break;
			}
		}
		return text;
	}

	public static string GetInnerXml(this XElement element)
	{
		if (element == null)
		{
			return "";
		}
		using XmlReader xmlReader = element.CreateReader();
		xmlReader.MoveToContent();
		return xmlReader.ReadInnerXml();
	}

	private static int GetElementIndexForXPath(XmlElement element, out bool multiple)
	{
		multiple = false;
		XmlNode parentNode = element.ParentNode;
		if (parentNode is XmlDocument)
		{
			return 1;
		}
		if (!(parentNode is XmlElement))
		{
			return 1;
		}
		foreach (XmlNode childNode in parentNode.ChildNodes)
		{
			if (childNode is XmlElement && childNode.Name == element.Name && childNode != element)
			{
				multiple = true;
				break;
			}
		}
		int num = 1;
		foreach (XmlNode childNode2 in parentNode.ChildNodes)
		{
			if (childNode2 is XmlElement && childNode2.Name == element.Name)
			{
				if (childNode2 == element)
				{
					return num;
				}
				num++;
			}
		}
		return 1;
	}
}
