using UnityEngine;
using Verse;

namespace RimWorld
{
	public class InspirationWorker
	{
		public InspirationDef def;

		private const float ChanceFactorPassionNone = 1f;

		private const float ChanceFactorPassionMinor = 2.5f;

		private const float ChanceFactorPassionMajor = 5f;

		public virtual float CommonalityFor(Pawn pawn)
		{
			float num = 1f;
			if (pawn.skills != null && def.associatedSkills != null)
			{
				for (int i = 0; i < def.associatedSkills.Count; i++)
				{
					SkillDef skillDef = def.associatedSkills[i];
					for (int j = 0; j < pawn.skills.skills.Count; j++)
					{
						SkillRecord skillRecord = pawn.skills.skills[j];
						if (skillDef == skillRecord.def)
						{
							switch (pawn.skills.skills[j].passion)
							{
							case Passion.None:
								num = Mathf.Max(num, 1f);
								break;
							case Passion.Minor:
								num = Mathf.Max(num, 2.5f);
								break;
							case Passion.Major:
								num = Mathf.Max(num, 5f);
								break;
							}
						}
					}
				}
			}
			return def.baseCommonality * num;
		}

		public virtual bool InspirationCanOccur(Pawn pawn)
		{
			if (!def.allowedOnAnimals && pawn.RaceProps.Animal)
			{
				return false;
			}
			if (!def.allowedOnNonColonists && !pawn.IsColonist)
			{
				return false;
			}
			if (!def.allowedOnDownedPawns && pawn.Downed)
			{
				return false;
			}
			if (def.requiredNonDisabledStats != null)
			{
				for (int i = 0; i < def.requiredNonDisabledStats.Count; i++)
				{
					if (def.requiredNonDisabledStats[i].Worker.IsDisabledFor(pawn))
					{
						return false;
					}
				}
			}
			if (def.requiredSkills != null)
			{
				for (int j = 0; j < def.requiredSkills.Count; j++)
				{
					if (!def.requiredSkills[j].PawnSatisfies(pawn))
					{
						return false;
					}
				}
			}
			if (!def.requiredAnySkill.NullOrEmpty())
			{
				bool flag = false;
				for (int k = 0; k < def.requiredAnySkill.Count; k++)
				{
					if (def.requiredAnySkill[k].PawnSatisfies(pawn))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			if (def.requiredNonDisabledWorkTypes != null)
			{
				for (int l = 0; l < def.requiredNonDisabledWorkTypes.Count; l++)
				{
					if (pawn.WorkTypeIsDisabled(def.requiredNonDisabledWorkTypes[l]))
					{
						return false;
					}
				}
			}
			if (!def.requiredAnyNonDisabledWorkType.NullOrEmpty())
			{
				bool flag2 = false;
				for (int m = 0; m < def.requiredAnyNonDisabledWorkType.Count; m++)
				{
					if (!pawn.WorkTypeIsDisabled(def.requiredAnyNonDisabledWorkType[m]))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					return false;
				}
			}
			if (def.requiredCapacities != null)
			{
				for (int n = 0; n < def.requiredCapacities.Count; n++)
				{
					if (!pawn.health.capacities.CapableOf(def.requiredCapacities[n]))
					{
						return false;
					}
				}
			}
			if (pawn.story != null)
			{
				for (int num = 0; num < pawn.story.traits.allTraits.Count; num++)
				{
					Trait trait = pawn.story.traits.allTraits[num];
					if (trait.CurrentData.disallowedInspirations != null && trait.CurrentData.disallowedInspirations.Contains(def))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
