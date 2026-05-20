using Verse;

namespace RimWorld
{
	public class CompProperties_Biocodable : CompProperties
	{
		public bool biocodeOnEquip;

		public CompProperties_Biocodable()
		{
			compClass = typeof(CompBiocodable);
		}
	}
}
