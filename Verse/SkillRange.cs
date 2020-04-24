using RimWorld;

namespace Verse
{
	public class SkillRange
	{
		private SkillDef skill;

		private IntRange range = IntRange.one;

		public SkillDef Skill => skill;

		public IntRange Range => range;
	}
}
