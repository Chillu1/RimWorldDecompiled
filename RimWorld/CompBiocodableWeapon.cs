using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompBiocodableWeapon : CompBiocodable
	{
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			if (biocoded)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Stat_Thing_Biocoded_Name".Translate(), codedPawnLabel, "Stat_Thing_Biocoded_Desc".Translate(), 5404);
			}
		}
	}
}
