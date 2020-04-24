using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_HunterHasShieldAndRangedWeapon : Alert
	{
		private List<Pawn> badHuntersResult = new List<Pawn>();

		private List<Pawn> BadHunters
		{
			get
			{
				badHuntersResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.workSettings.WorkIsActive(WorkTypeDefOf.Hunting) && WorkGiver_HunterHunt.HasShieldAndRangedWeapon(item))
					{
						badHuntersResult.Add(item);
					}
				}
				return badHuntersResult;
			}
		}

		public Alert_HunterHasShieldAndRangedWeapon()
		{
			defaultLabel = "HunterHasShieldAndRangedWeapon".Translate();
			defaultExplanation = "HunterHasShieldAndRangedWeaponDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BadHunters);
		}
	}
}
