using System;
using System.Xml;
using RimWorld;

namespace Verse;

public class DefHyperlink
{
	public Def def;

	public Faction faction;

	public DefHyperlink()
	{
	}

	public DefHyperlink(Def def)
	{
		this.def = def;
	}

	public DefHyperlink(RoyalTitleDef def, Faction faction)
	{
		this.def = def;
		this.faction = faction;
	}

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		if (xmlRoot.ChildNodes.Count != 1)
		{
			Log.Error("Misconfigured DefHyperlink: " + xmlRoot.OuterXml);
			return;
		}
		Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(xmlRoot.Name);
		if (typeInAnyAssembly == null)
		{
			Log.Error("Misconfigured DefHyperlink. Could not find def of type " + xmlRoot.Name);
		}
		else
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "def", xmlRoot.FirstChild.Value, null, null, typeInAnyAssembly);
		}
	}

	public static implicit operator DefHyperlink(Def def)
	{
		return new DefHyperlink
		{
			def = def
		};
	}
}
