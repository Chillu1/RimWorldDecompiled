using RimWorld;

namespace Verse;

public class PassionMod
{
	public enum PassionModType
	{
		None,
		AddOneLevel,
		DropAll
	}

	public SkillDef skill;

	public PassionModType modType;

	public PassionMod()
	{
	}

	public PassionMod(SkillDef skill, PassionModType modType)
	{
		this.skill = skill;
		this.modType = modType;
	}

	public Passion NewPassionFor(SkillRecord skillRecord)
	{
		switch (modType)
		{
		case PassionModType.AddOneLevel:
			switch (skillRecord.passion)
			{
			case Passion.None:
				return Passion.Minor;
			case Passion.Minor:
				return Passion.Major;
			}
			break;
		case PassionModType.DropAll:
			return Passion.None;
		}
		return skillRecord.passion;
	}
}
