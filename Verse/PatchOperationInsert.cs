using System.Xml;

namespace Verse;

public class PatchOperationInsert : PatchOperationPathed
{
	private enum Order
	{
		Append,
		Prepend
	}

	private XmlContainer value;

	private Order order = Order.Prepend;

	protected override bool ApplyWorker(XmlDocument xml)
	{
		XmlNode node = value.node;
		bool result = false;
		foreach (object item in xml.SelectNodes(xpath))
		{
			result = true;
			XmlNode xmlNode = item as XmlNode;
			XmlNode parentNode = xmlNode.ParentNode;
			if (order == Order.Append)
			{
				foreach (XmlNode childNode in node.ChildNodes)
				{
					parentNode.InsertAfter(parentNode.OwnerDocument.ImportNode(childNode, deep: true), xmlNode);
				}
			}
			else if (order == Order.Prepend)
			{
				for (int num = node.ChildNodes.Count - 1; num >= 0; num--)
				{
					parentNode.InsertBefore(parentNode.OwnerDocument.ImportNode(node.ChildNodes[num], deep: true), xmlNode);
				}
			}
			if (xmlNode.NodeType == XmlNodeType.Text)
			{
				parentNode.Normalize();
			}
		}
		return result;
	}
}
