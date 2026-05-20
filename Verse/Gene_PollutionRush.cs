using RimWorld;

namespace Verse
{
	public class Gene_PollutionRush : Gene
	{
		public override void PostRemove()
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PollutionStimulus);
			if (firstHediffOfDef != null)
			{
				pawn.health.RemoveHediff(firstHediffOfDef);
			}
			base.PostRemove();
		}
	}
}
