using System.Xml;
using Verse;

namespace RimWorld
{
	public class BackstoryTrait
	{
		public TraitDef def;

		public int degree;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.Name);
			if (xmlRoot.HasChildNodes)
			{
				degree = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
			}
			else
			{
				degree = 0;
			}
		}
	}
}
