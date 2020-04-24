using RimWorld;

namespace Verse
{
	public class Hediff_Alcohol : HediffWithComps
	{
		private const int HangoverCheckInterval = 300;

		public override void Tick()
		{
			base.Tick();
			if (CurStageIndex >= 3 && pawn.IsHashIntervalTick(300) && HangoverSusceptible(pawn))
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hangover);
				if (firstHediffOfDef != null)
				{
					firstHediffOfDef.Severity = 1f;
					return;
				}
				firstHediffOfDef = HediffMaker.MakeHediff(HediffDefOf.Hangover, pawn);
				firstHediffOfDef.Severity = 1f;
				pawn.health.AddHediff(firstHediffOfDef);
			}
		}

		private bool HangoverSusceptible(Pawn pawn)
		{
			return true;
		}
	}
}
