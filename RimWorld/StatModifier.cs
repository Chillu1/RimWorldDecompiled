using System.Xml;
using Verse;

namespace RimWorld;

public class StatModifier
{
	public StatDef stat;

	public float value;

	public string ValueToStringAsOffset => stat.Worker.ValueToString(value, finalized: false, ToStringNumberSense.Offset);

	public string ToStringAsFactor => stat.Worker.ValueToString(value, finalized: false, ToStringNumberSense.Factor);

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "stat", xmlRoot.Name);
		value = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
	}

	public override string ToString()
	{
		if (stat == null)
		{
			return "(null stat)";
		}
		return stat.defName + "-" + value;
	}
}
