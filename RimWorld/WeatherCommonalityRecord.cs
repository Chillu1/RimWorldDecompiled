using System.Xml;
using Verse;

namespace RimWorld;

public class WeatherCommonalityRecord
{
	public WeatherDef weather;

	public float commonality;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "weather", xmlRoot);
		commonality = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
	}
}
