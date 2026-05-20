using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Recipe_ImplantEmbryo : Recipe_Surgery
{
	public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
	{
		return false;
	}

	public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
	{
		if (!ModsConfig.BiotechActive)
		{
			return;
		}
		HumanEmbryo humanEmbryo = (HumanEmbryo)ingredients.FirstOrDefault((Thing t) => t.def == ThingDefOf.HumanEmbryo);
		if (humanEmbryo == null)
		{
			Log.Warning($"Tried to perform implant embryo surgery on {pawn} but no embryo was in the ingredients list");
		}
		else if (!CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
		{
			if (Rand.Chance(PregnancyUtility.PregnancyChanceImplantEmbryo(pawn)))
			{
				Hediff_Pregnant hediff_Pregnant = (Hediff_Pregnant)HediffMaker.MakeHediff(HediffDefOf.PregnantHuman, pawn);
				hediff_Pregnant.SetParents(humanEmbryo.Mother, humanEmbryo.Father, humanEmbryo.GeneSet);
				pawn.health.AddHediff(hediff_Pregnant);
			}
			else
			{
				Messages.Message("ImplantFailedMessage".Translate(humanEmbryo.Label, pawn), pawn, MessageTypeDefOf.NegativeHealthEvent);
			}
		}
	}

	public override string LabelFromUniqueIngredients(Bill bill)
	{
		if (bill is Bill_Medical bill_Medical && !bill_Medical.uniqueRequiredIngredients.NullOrEmpty())
		{
			foreach (Thing uniqueRequiredIngredient in bill_Medical.uniqueRequiredIngredients)
			{
				if (uniqueRequiredIngredient is HumanEmbryo humanEmbryo && !humanEmbryo.DestroyedOrNull())
				{
					return "ImplantSurgeryLabel".Translate(humanEmbryo.Label);
				}
			}
		}
		return null;
	}

	public override bool CompletableEver(Pawn surgeryTarget)
	{
		if (!base.CompletableEver(surgeryTarget))
		{
			return false;
		}
		if (surgeryTarget.Sterile())
		{
			return false;
		}
		return true;
	}
}
