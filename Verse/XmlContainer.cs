using System.Xml;

namespace Verse
{
	public class XmlContainer
	{
		public XmlNode node;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			node = xmlRoot;
		}
	}
}
