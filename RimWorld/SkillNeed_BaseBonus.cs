using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SkillNeed_BaseBonus : SkillNeed
	{
		private float baseValue = 0.5f;

		private float bonusPerLevel = 0.05f;

		public override float ValueFor(Pawn pawn)
		{
			if (pawn.skills == null)
			{
				return 1f;
			}
			int level = pawn.skills.GetSkill(skill).Level;
			return ValueAtLevel(level);
		}

		private float ValueAtLevel(int level)
		{
			return baseValue + bonusPerLevel * (float)level;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			foreach (string item in base.ConfigErrors())
			{
				yield return item;
			}
			for (int i = 1; i <= 20; i++)
			{
				if (ValueAtLevel(i) <= 0f)
				{
					yield return "SkillNeed yields factor < 0 at skill level " + i;
				}
			}
		}
	}
}
