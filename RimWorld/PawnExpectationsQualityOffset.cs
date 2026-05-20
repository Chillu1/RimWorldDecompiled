using System.Xml;
using Verse;

namespace RimWorld
{
	public class PawnExpectationsQualityOffset
	{
		public ExpectationDef expectation;

		public float offset;

		[MustTranslate]
		public string labelOverride;

		public string Label => labelOverride ?? ((string)expectation.LabelCap);

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "expectation", xmlRoot.Name);
			offset = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}
	}
}
