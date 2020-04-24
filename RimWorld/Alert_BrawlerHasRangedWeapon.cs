using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_BrawlerHasRangedWeapon : Alert
	{
		private List<Pawn> brawlersWithRangedWeaponResult = new List<Pawn>();

		private List<Pawn> BrawlersWithRangedWeapon
		{
			get
			{
				brawlersWithRangedWeaponResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.story.traits.HasTrait(TraitDefOf.Brawler) && item.equipment.Primary != null && item.equipment.Primary.def.IsRangedWeapon)
					{
						brawlersWithRangedWeaponResult.Add(item);
					}
				}
				return brawlersWithRangedWeaponResult;
			}
		}

		public Alert_BrawlerHasRangedWeapon()
		{
			defaultLabel = "BrawlerHasRangedWeapon".Translate();
			defaultExplanation = "BrawlerHasRangedWeaponDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BrawlersWithRangedWeapon);
		}
	}
}
