using System.Xml;
using Verse;

namespace RimWorld;

public class MeditationFocusOffsetPerBuilding
{
	public ThingDef building;

	public float offset;

	public MeditationFocusOffsetPerBuilding()
	{
	}

	public MeditationFocusOffsetPerBuilding(ThingDef building, float offset)
	{
		this.building = building;
		this.offset = offset;
	}

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		if (xmlRoot.Name != "li")
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "building", xmlRoot);
			offset = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}
		else
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "building", xmlRoot.InnerText);
			offset = float.MinValue;
		}
	}
}
