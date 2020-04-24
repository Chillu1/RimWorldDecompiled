using System.Xml;

namespace Verse
{
	public class PatchOperationAttributeRemove : PatchOperationAttribute
	{
		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool result = false;
			foreach (object item in xml.SelectNodes(xpath))
			{
				XmlNode xmlNode = item as XmlNode;
				if (xmlNode.Attributes[attribute] != null)
				{
					xmlNode.Attributes.Remove(xmlNode.Attributes[attribute]);
					result = true;
				}
			}
			return result;
		}
	}
}
