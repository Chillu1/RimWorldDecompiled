using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_HunterLacksRangedWeapon : Alert
	{
		private List<Pawn> huntersWithoutRangedWeaponResult = new List<Pawn>();

		private List<Pawn> HuntersWithoutRangedWeapon
		{
			get
			{
				huntersWithoutRangedWeaponResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) && !WorkGiver_HunterHunt.HasHuntingWeapon(item) && !item.Downed)
					{
						huntersWithoutRangedWeaponResult.Add(item);
					}
				}
				return huntersWithoutRangedWeaponResult;
			}
		}

		public Alert_HunterLacksRangedWeapon()
		{
			defaultLabel = "HunterLacksWeapon".Translate();
			defaultExplanation = "HunterLacksWeaponDesc".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(HuntersWithoutRangedWeapon);
		}
	}
}
