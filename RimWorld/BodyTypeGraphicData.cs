using System.Xml;
using Verse;

namespace RimWorld;

public class BodyTypeGraphicData
{
	public BodyTypeDef bodyType;

	public string texturePath;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "bodyType", xmlRoot.Name);
		texturePath = (xmlRoot.HasChildNodes ? ParseHelper.FromString<string>(xmlRoot.FirstChild.Value) : null);
	}
}
