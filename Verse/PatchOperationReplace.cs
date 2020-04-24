using System.Linq;
using System.Xml;

namespace Verse
{
	public class PatchOperationReplace : PatchOperationPathed
	{
		private XmlContainer value;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = value.node;
			bool result = false;
			XmlNode[] array = xml.SelectNodes(xpath).Cast<XmlNode>().ToArray();
			foreach (XmlNode xmlNode in array)
			{
				result = true;
				XmlNode parentNode = xmlNode.ParentNode;
				foreach (XmlNode childNode in node.ChildNodes)
				{
					parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(childNode, deep: true), xmlNode);
				}
				parentNode.RemoveChild(xmlNode);
			}
			return result;
		}
	}
}
