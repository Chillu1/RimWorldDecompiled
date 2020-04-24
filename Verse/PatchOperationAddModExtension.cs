using System.Xml;

namespace Verse
{
	public class PatchOperationAddModExtension : PatchOperationPathed
	{
		private XmlContainer value;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = value.node;
			bool result = false;
			foreach (object item in xml.SelectNodes(xpath))
			{
				XmlNode xmlNode = item as XmlNode;
				XmlNode xmlNode2 = xmlNode["modExtensions"];
				if (xmlNode2 == null)
				{
					xmlNode2 = xmlNode.OwnerDocument.CreateElement("modExtensions");
					xmlNode.AppendChild(xmlNode2);
				}
				foreach (XmlNode childNode in node.ChildNodes)
				{
					xmlNode2.AppendChild(xmlNode.OwnerDocument.ImportNode(childNode, deep: true));
				}
				result = true;
			}
			return result;
		}
	}
}
