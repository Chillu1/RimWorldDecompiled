using Verse;

namespace RimWorld
{
	public class CompProperties_Hatcher : CompProperties
	{
		public float hatcherDaystoHatch = 1f;

		public PawnKindDef hatcherPawn;

		public CompProperties_Hatcher()
		{
			compClass = typeof(CompHatcher);
		}
	}
}
