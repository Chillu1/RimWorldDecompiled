using System.Xml;
using Verse;

namespace RimWorld;

public class SkillGain
{
	public SkillDef skill;

	public int amount;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
		if (xmlRoot.HasChildNodes)
		{
			amount = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
		}
	}
}
