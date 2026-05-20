using RimWorld;

namespace Verse.AI;

public class Verb_CastTargetEffectBiomutationLance : Verb_CastTargetEffect
{
	public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
	{
		Pawn pawn = target.Pawn;
		if (pawn != null)
		{
			if (!pawn.RaceProps.Humanlike && !pawn.IsAnimal)
			{
				if (showMessages)
				{
					Messages.Message("MessageBiomutationLanceInvalidTargetRace".Translate(pawn), caster, MessageTypeDefOf.RejectInput, null, historical: false);
				}
				return false;
			}
			if (pawn.BodySize > 2.5f)
			{
				if (showMessages)
				{
					Messages.Message("MessageBiomutationLanceTargetTooBig".Translate(pawn), caster, MessageTypeDefOf.RejectInput, null, historical: false);
				}
				return false;
			}
		}
		return base.ValidateTarget(target, showMessages);
	}
}
