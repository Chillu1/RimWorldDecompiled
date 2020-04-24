using System.Xml;

namespace Verse
{
	public class PatchOperationAdd : PatchOperationPathed
	{
		private enum Order
		{
			Append,
			Prepend
		}

		private XmlContainer value;

		private Order order;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			XmlNode node = value.node;
			bool result = false;
			foreach (object item in xml.SelectNodes(xpath))
			{
				result = true;
				XmlNode xmlNode = item as XmlNode;
				if (order == Order.Append)
				{
					foreach (XmlNode childNode in node.ChildNodes)
					{
						xmlNode.AppendChild(xmlNode.OwnerDocument.ImportNode(childNode, deep: true));
					}
				}
				else if (order == Order.Prepend)
				{
					for (int num = node.ChildNodes.Count - 1; num >= 0; num--)
					{
						xmlNode.PrependChild(xmlNode.OwnerDocument.ImportNode(node.ChildNodes[num], deep: true));
					}
				}
			}
			return result;
		}
	}
}
