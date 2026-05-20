using System;
using System.Xml;
using Verse;

namespace RimWorld;

public class MutatorChance
{
	public TileMutatorDef mutator;

	public float chance = 1f;

	public bool required;

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "mutator", xmlRoot.Name);
		required = xmlRoot.Attributes["Required"]?.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ?? false;
		if (xmlRoot.HasChildNodes)
		{
			chance = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
		}
		else
		{
			chance = 1f;
		}
	}
}
