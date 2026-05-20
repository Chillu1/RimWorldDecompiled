using System.Xml;

namespace Verse
{
	public class MechWorkTypePriority
	{
		public WorkTypeDef def;

		public int priority;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);
			priority = (xmlRoot.HasChildNodes ? ParseHelper.FromString<int>(xmlRoot.FirstChild.Value) : 3);
		}
	}
}
