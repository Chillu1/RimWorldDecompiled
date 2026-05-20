using UnityEngine;
using Verse;

namespace RimWorld;

public class CompAbilityEffect_Resurrect : CompAbilityEffect
{
	public new CompProperties_Resurrect Props => (CompProperties_Resurrect)props;

	public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
	{
		base.Apply(target, dest);
		Pawn innerPawn = ((Corpse)target.Thing).InnerPawn;
		if (ResurrectionUtility.TryResurrectWithSideEffects(innerPawn))
		{
			Messages.Message("MessagePawnResurrected".Translate(innerPawn), innerPawn, MessageTypeDefOf.PositiveEvent);
			MoteMaker.MakeAttachedOverlay(innerPawn, ThingDefOf.Mote_ResurrectFlash, Vector3.zero);
		}
	}

	public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
	{
		if (target.HasThing && target.Thing is Corpse corpse && corpse.GetRotStage() == RotStage.Dessicated)
		{
			if (throwMessages)
			{
				Messages.Message("MessageCannotResurrectDessicatedCorpse".Translate(), corpse, MessageTypeDefOf.RejectInput, historical: false);
			}
			return false;
		}
		return base.Valid(target, throwMessages);
	}
}
