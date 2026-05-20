using System.Xml;
using Verse;

namespace RimWorld;

public class BiomeAnimalRecord
{
	public PawnKindDef animal;

	public float commonality;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "animal", xmlRoot);
		commonality = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
	}
}
