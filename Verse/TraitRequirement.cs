using System.Xml;
using RimWorld;

namespace Verse;

public class TraitRequirement
{
	public TraitDef def;

	public int? degree;

	public bool Matches(Trait trait)
	{
		if (trait.def == def)
		{
			if (degree.HasValue)
			{
				return trait.Degree == degree.Value;
			}
			return true;
		}
		return false;
	}

	public bool HasTrait(Pawn p)
	{
		if (p.story == null)
		{
			return false;
		}
		if (!degree.HasValue)
		{
			return p.story.traits.HasTrait(def);
		}
		return p.story.traits.HasTrait(def, degree.Value);
	}

	public Trait GetTrait(Pawn p)
	{
		if (p.story == null)
		{
			return null;
		}
		if (!degree.HasValue)
		{
			return p.story.traits.GetTrait(def);
		}
		return p.story.traits.GetTrait(def, degree.Value);
	}

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		XmlHelper.ParseElements(this, xmlRoot, "def", "degree");
	}
}
