using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class SappersUtility
	{
		public static bool IsGoodSapper(Pawn p)
		{
			if (p.kindDef.canBeSapper && HasBuildingDestroyerWeapon(p))
			{
				return CanMineReasonablyFast(p);
			}
			return false;
		}

		public static bool IsGoodBackupSapper(Pawn p)
		{
			if (p.kindDef.canBeSapper)
			{
				return CanMineReasonablyFast(p);
			}
			return false;
		}

		private static bool CanMineReasonablyFast(Pawn p)
		{
			if (p.RaceProps.Humanlike && !p.skills.GetSkill(SkillDefOf.Mining).TotallyDisabled && !StatDefOf.MiningSpeed.Worker.IsDisabledFor(p))
			{
				return p.skills.GetSkill(SkillDefOf.Mining).Level >= 4;
			}
			return false;
		}

		public static bool HasBuildingDestroyerWeapon(Pawn p)
		{
			if (p.equipment == null || p.equipment.Primary == null)
			{
				return false;
			}
			List<Verb> allVerbs = p.equipment.Primary.GetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				if (allVerbs[i].verbProps.ai_IsBuildingDestroyer)
				{
					return true;
				}
			}
			return false;
		}
	}
}
