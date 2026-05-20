using System.Collections.Generic;
using Verse;

namespace RimWorld;

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
				ThingWithComps primary = item.equipment.Primary;
				if (primary == null || !primary.def.IsRangedWeapon)
				{
					continue;
				}
				bool flag = false;
				List<VerbProperties> verbs = primary.def.Verbs;
				for (int i = 0; i < verbs.Count; i++)
				{
					if (typeof(Verb_LaunchProjectile).IsAssignableFrom(verbs[i].verbClass))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					continue;
				}
				List<Apparel> wornApparel = item.apparel.WornApparel;
				for (int j = 0; j < wornApparel.Count; j++)
				{
					if (wornApparel[j].def.IsShieldThatBlocksRanged)
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
