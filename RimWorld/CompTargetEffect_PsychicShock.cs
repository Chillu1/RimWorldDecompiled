using Verse;

namespace RimWorld
{
	public class CompTargetEffect_PsychicShock : CompTargetEffect
	{
		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (!pawn.Dead)
			{
				Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
				BodyPartRecord result = null;
				pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource).TryRandomElement(out result);
				pawn.health.AddHediff(hediff, result);
			}
		}
	}
}
