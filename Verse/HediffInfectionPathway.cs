using System.Xml;
using RimWorld;

namespace Verse;

public class HediffInfectionPathway
{
	private InfectionPathwayDef pathwayDef;

	[MustTranslate]
	private string explanation;

	public InfectionPathwayDef PathwayDef => pathwayDef;

	public string Explanation => explanation;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		if (xmlRoot.Name == "li")
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "pathwayDef", xmlRoot.FirstChild.Value);
			return;
		}
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "pathwayDef", xmlRoot.Name);
		if (xmlRoot.HasChildNodes)
		{
			explanation = ParseHelper.FromString<string>(xmlRoot.FirstChild.Value);
		}
	}
}
