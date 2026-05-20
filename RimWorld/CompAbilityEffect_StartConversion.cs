using Verse;

namespace RimWorld
{
	public class CompAbilityEffect_StartConversion : CompAbilityEffect_StartRitualOnPawn
	{
		public new CompProperties_AbilityStartConversion Props => (CompProperties_AbilityStartConversion)props;

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			if (!RitualBehaviorWorker_Conversion.ValidateConvertee(target.Pawn, parent.pawn, throwMessages))
			{
				return false;
			}
			return base.Valid(target, throwMessages);
		}
	}
}
