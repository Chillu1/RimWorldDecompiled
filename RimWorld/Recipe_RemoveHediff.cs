using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_RemoveHediff : Recipe_Surgery
{
	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		if (!base.AvailableOnNow(thing, part))
		{
			return false;
		}
		if (!(thing is Pawn pawn))
		{
			return false;
		}
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if ((!recipe.targetsBodyPart || hediffs[i].Part != null) && hediffs[i].def == recipe.removesHediff && hediffs[i].Visible)
			{
				return true;
			}
		}
		return false;
	}

	public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
	{
		List<Hediff> allHediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < allHediffs.Count; i++)
		{
			if (allHediffs[i].Part != null && allHediffs[i].def == recipe.removesHediff && allHediffs[i].Visible)
			{
				yield return allHediffs[i].Part;
			}
		}
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (billDoer != null)
		{
			if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
			{
				return;
			}
			TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
			if (PawnUtility.ShouldSendNotificationAbout(pawn) || PawnUtility.ShouldSendNotificationAbout(billDoer))
			{
				string text = (recipe.successfullyRemovedHediffMessage.NullOrEmpty() ? ((string)"MessageSuccessfullyRemovedHediff".Translate(billDoer.LabelShort, pawn.LabelShort, recipe.removesHediff.label.Named("HEDIFF"), billDoer.Named("SURGEON"), pawn.Named("PATIENT"))) : ((string)recipe.successfullyRemovedHediffMessage.Formatted(billDoer.LabelShort, pawn.LabelShort)));
				Messages.Message(text, pawn, MessageTypeDefOf.PositiveEvent);
			}
		}
		if (recipe.targetsBodyPart)
		{
			Hediff hediff = pawn.health.hediffSet.hediffs.Find((Hediff x) => x.def == recipe.removesHediff && x.Part == part && x.Visible);
			if (hediff != null)
			{
				pawn.health.RemoveHediff(hediff);
			}
			return;
		}
		for (int num = pawn.health.hediffSet.hediffs.Count - 1; num >= 0; num--)
		{
			Hediff hediff2 = pawn.health.hediffSet.hediffs[num];
			if (hediff2.def == recipe.removesHediff && hediff2.Visible)
			{
				pawn.health.RemoveHediff(hediff2);
			}
		}
	}
}
