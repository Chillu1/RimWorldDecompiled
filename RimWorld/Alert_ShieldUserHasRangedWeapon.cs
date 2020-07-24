using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_ShieldUserHasRangedWeapon : Alert
	{
		private List<Pawn> shieldUsersWithRangedWeaponResult = new List<Pawn>();

		private List<Pawn> ShieldUsersWithRangedWeapon
		{
			get
			{
				shieldUsersWithRangedWeaponResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.equipment.Primary == null || !item.equipment.Primary.def.IsRangedWeapon)
					{
						continue;
					}
					List<Apparel> wornApparel = item.apparel.WornApparel;
					for (int i = 0; i < wornApparel.Count; i++)
					{
						if (wornApparel[i] is ShieldBelt)
						{
							shieldUsersWithRangedWeaponResult.Add(item);
							break;
						}
					}
				}
				return shieldUsersWithRangedWeaponResult;
			}
		}

		public Alert_ShieldUserHasRangedWeapon()
		{
			defaultLabel = "ShieldUserHasRangedWeapon".Translate();
			defaultExplanation = "ShieldUserHasRangedWeaponDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(ShieldUsersWithRangedWeapon);
		}
	}
}
