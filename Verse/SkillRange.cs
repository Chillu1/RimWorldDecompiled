using RimWorld;

namespace Verse;

public class SkillRange
{
	private SkillDef skill;

	private IntRange range = IntRange.One;

	public SkillDef Skill => skill;

	public IntRange Range => range;
}
