using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Recipe_Surgery : RecipeWorker
	{
		private const float MaxSuccessChance = 0.98f;

		private const float CatastrophicFailChance = 0.5f;

		private const float RidiculousFailChanceFromCatastrophic = 0.1f;

		private const float InspiredSurgerySuccessChanceFactor = 2f;

		private static readonly SimpleCurve MedicineMedicalPotencyToSurgeryChanceFactor = new SimpleCurve
		{
			new CurvePoint(0f, 0.7f),
			new CurvePoint(1f, 1f),
			new CurvePoint(2f, 1.3f)
		};

		protected bool CheckSurgeryFail(Pawn surgeon, Pawn patient, List<Thing> ingredients, BodyPartRecord part, Bill bill)
		{
			if (bill.recipe.surgerySuccessChanceFactor >= 99999f)
			{
				return false;
			}
			float num = 1f;
			if (!patient.RaceProps.IsMechanoid)
			{
				num *= surgeon.GetStatValue(StatDefOf.MedicalSurgerySuccessChance);
			}
			if (patient.InBed())
			{
				num *= patient.CurrentBed().GetStatValue(StatDefOf.SurgerySuccessChanceFactor);
			}
			num *= MedicineMedicalPotencyToSurgeryChanceFactor.Evaluate(GetAverageMedicalPotency(ingredients, bill));
			num *= recipe.surgerySuccessChanceFactor;
			if (surgeon.InspirationDef == InspirationDefOf.Inspired_Surgery && !patient.RaceProps.IsMechanoid)
			{
				num *= 2f;
				surgeon.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Surgery);
			}
			num = Mathf.Min(num, 0.98f);
			if (!Rand.Chance(num))
			{
				if (Rand.Chance(recipe.deathOnFailedSurgeryChance))
				{
					HealthUtility.GiveInjuriesOperationFailureCatastrophic(patient, part);
					if (!patient.Dead)
					{
						patient.Kill(null, null);
					}
					Messages.Message("MessageMedicalOperationFailureFatal".Translate(surgeon.LabelShort, patient.LabelShort, recipe.LabelCap, surgeon.Named("SURGEON"), patient.Named("PATIENT")), patient, MessageTypeDefOf.NegativeHealthEvent);
				}
				else if (Rand.Chance(0.5f))
				{
					if (Rand.Chance(0.1f))
					{
						Messages.Message("MessageMedicalOperationFailureRidiculous".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), patient, MessageTypeDefOf.NegativeHealthEvent);
						HealthUtility.GiveInjuriesOperationFailureRidiculous(patient);
					}
					else
					{
						Messages.Message("MessageMedicalOperationFailureCatastrophic".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), patient, MessageTypeDefOf.NegativeHealthEvent);
						HealthUtility.GiveInjuriesOperationFailureCatastrophic(patient, part);
					}
				}
				else
				{
					Messages.Message("MessageMedicalOperationFailureMinor".Translate(surgeon.LabelShort, patient.LabelShort, surgeon.Named("SURGEON"), patient.Named("PATIENT"), recipe.Named("RECIPE")), patient, MessageTypeDefOf.NegativeHealthEvent);
					HealthUtility.GiveInjuriesOperationFailureMinor(patient, part);
				}
				if (!patient.Dead)
				{
					TryGainBotchedSurgeryThought(patient, surgeon);
				}
				return true;
			}
			return false;
		}

		private void TryGainBotchedSurgeryThought(Pawn patient, Pawn surgeon)
		{
			if (patient.RaceProps.Humanlike && patient.needs.mood != null)
			{
				patient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BotchedMySurgery, surgeon);
			}
		}

		private float GetAverageMedicalPotency(List<Thing> ingredients, Bill bill)
		{
			ThingDef thingDef = (bill as Bill_Medical)?.consumedInitialMedicineDef;
			int num = 0;
			float num2 = 0f;
			if (thingDef != null)
			{
				num++;
				num2 += thingDef.GetStatValueAbstract(StatDefOf.MedicalPotency);
			}
			for (int i = 0; i < ingredients.Count; i++)
			{
				Medicine medicine = ingredients[i] as Medicine;
				if (medicine != null)
				{
					num += medicine.stackCount;
					num2 += medicine.GetStatValue(StatDefOf.MedicalPotency) * (float)medicine.stackCount;
				}
			}
			if (num == 0)
			{
				return 1f;
			}
			return num2 / (float)num;
		}
	}
}
