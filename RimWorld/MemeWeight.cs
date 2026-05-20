using System.Xml;
using Verse;

namespace RimWorld
{
	public class MemeWeight
	{
		public MemeDef meme;

		public float selectionWeight;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "meme", xmlRoot.Name);
			selectionWeight = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}
	}
}
