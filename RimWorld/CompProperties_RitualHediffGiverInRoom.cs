using Verse;

namespace RimWorld
{
	public class CompProperties_RitualHediffGiverInRoom : CompProperties
	{
		public HediffDef hediff;

		public float minRadius = 999f;

		public float severity = -1f;

		public bool resetLastRecreationalDrugTick;

		public CompProperties_RitualHediffGiverInRoom()
		{
			compClass = typeof(CompRitualHediffGiverInRoom);
		}
	}
}
