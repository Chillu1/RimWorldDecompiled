using System.Xml;

namespace Verse
{
	public class PatchOperationTest : PatchOperationPathed
	{
		protected override bool ApplyWorker(XmlDocument xml)
		{
			return xml.SelectSingleNode(xpath) != null;
		}
	}
}
