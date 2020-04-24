using System.Linq;
using System.Xml;

namespace Verse
{
	public class PatchOperationSetName : PatchOperationPathed
	{
		protected string name;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool result = false;
			XmlNode[] array = xml.SelectNodes(xpath).Cast<XmlNode>().ToArray();
			foreach (XmlNode xmlNode in array)
			{
				result = true;
				XmlNode xmlNode2 = xmlNode.OwnerDocument.CreateElement(name);
				xmlNode2.InnerXml = xmlNode.InnerXml;
				xmlNode.ParentNode.InsertBefore(xmlNode2, xmlNode);
				xmlNode.ParentNode.RemoveChild(xmlNode);
			}
			return result;
		}
	}
}
