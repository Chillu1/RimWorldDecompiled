using System.Linq;
using System.Xml;

namespace Verse
{
	public class PatchOperationRemove : PatchOperationPathed
	{
		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool result = false;
			XmlNode[] array = xml.SelectNodes(xpath).Cast<XmlNode>().ToArray();
			foreach (XmlNode xmlNode in array)
			{
				result = true;
				xmlNode.ParentNode.RemoveChild(xmlNode);
			}
			return result;
		}
	}
}
