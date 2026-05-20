using System.Xml;
using RimWorld;

namespace Verse;

public class SkillRequirement
{
	public SkillDef skill;

	public int minLevel;

	public string Summary
	{
		get
		{
			if (skill == null)
			{
				return "";
			}
			return $"{skill.LabelCap} ({minLevel})";
		}
	}

	public bool PawnSatisfies(Pawn pawn)
	{
		if (pawn.IsColonyMechPlayerControlled && pawn.RaceProps.mechFixedSkillLevel >= minLevel && pawn.RaceProps.mechEnabledWorkTypes.Any((WorkTypeDef w) => w.relevantSkills.NotNullAndContains(skill)))
		{
			return true;
		}
		if (pawn.skills == null)
		{
			return false;
		}
		return pawn.skills.GetSkill(skill).Level >= minLevel;
	}

	public void LoadDataFromXmlCustom(XmlNode xmlRoot)
	{
		DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
		minLevel = ParseHelper.FromString<int>(xmlRoot.FirstChild.Value);
	}

	public override string ToString()
	{
		if (skill == null)
		{
			return "null-skill-requirement";
		}
		return skill.defName + "-" + minLevel;
	}
}
