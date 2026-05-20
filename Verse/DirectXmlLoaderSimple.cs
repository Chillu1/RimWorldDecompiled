using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using RimWorld.IO;

namespace Verse;

public static class DirectXmlLoaderSimple
{
	public struct XmlKeyValuePair
	{
		public string key;

		public string value;

		public int lineNumber;
	}

	public static IEnumerable<XmlKeyValuePair> ValuesFromXmlFile(string fileContents)
	{
		return ValuesFromXmlFile(VirtualFileInfoExt.LoadAsXDocument(fileContents));
	}

	public static IEnumerable<XmlKeyValuePair> ValuesFromXmlFile(VirtualFile file)
	{
		return ValuesFromXmlFile(file.LoadAsXDocument());
	}

	public static IEnumerable<XmlKeyValuePair> ValuesFromXmlFile(XDocument doc)
	{
		foreach (XElement item in doc.Root.Elements())
		{
			string key = item.Name.ToString();
			string value = item.Value;
			value = value.Replace("\\n", "\n");
			yield return new XmlKeyValuePair
			{
				key = key,
				value = value,
				lineNumber = ((IXmlLineInfo)item).LineNumber
			};
		}
	}
}
