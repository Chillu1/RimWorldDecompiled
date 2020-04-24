using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class SkillNeed_Direct : SkillNeed
	{
		public List<float> valuesPerLevel = new List<float>();

		public override float ValueFor(Pawn pawn)
		{
			if (pawn.skills == null)
			{
				return 1f;
			}
			int level = pawn.skills.GetSkill(skill).Level;
			if (valuesPerLevel.Count > level)
			{
				return valuesPerLevel[level];
			}
			if (valuesPerLevel.Count > 0)
			{
				return valuesPerLevel[valuesPerLevel.Count - 1];
			}
			return 1f;
		}
	}
}
