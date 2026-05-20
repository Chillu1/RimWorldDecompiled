using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld;

public static class TendUtility
{
	public const float NoMedicinePotency = 0.3f;

	public const float NoMedicineQualityMax = 0.7f;

	public const float NoDoctorTendQuality = 0.75f;

	public const float SelfTendQualityFactor = 0.7f;

	private const float ChanceToDevelopBondRelationOnTended = 0.004f;

	private static List<Hediff> tmpHediffsToTend = new List<Hediff>();

	private static List<Hediff> tmpHediffs = new List<Hediff>();

	private static List<Pair<Hediff, float>> tmpHediffsWithTendPriority = new List<Pair<Hediff, float>>();

	public static void DoTend(Pawn doctor, Pawn patient, Medicine medicine)
	{
		if (!patient.health.HasHediffsNeedingTend())
		{
			return;
		}
		if (medicine != null && medicine.Destroyed)
		{
			Log.Warning("Tried to use destroyed medicine.");
			medicine = null;
		}
		float quality = CalculateBaseTendQuality(doctor, patient, medicine?.def);
		GetOptimalHediffsToTendWithSingleTreatment(patient, medicine != null, tmpHediffsToTend);
		float maxQuality = medicine?.def.GetStatValueAbstract(StatDefOf.MedicalQualityMax) ?? 0.7f;
		for (int i = 0; i < tmpHediffsToTend.Count; i++)
		{
			tmpHediffsToTend[i].Tended(quality, maxQuality, i);
		}
		if (doctor != null && doctor.Faction == Faction.OfPlayer && patient.Faction != doctor.Faction && !patient.IsPrisoner && patient.Faction != null)
		{
			patient.mindState.timesGuestTendedToByPlayer++;
		}
		if (doctor != null && doctor.RaceProps.Humanlike && patient.RaceProps.Animal && patient.RaceProps.playerCanChangeMaster && RelationsUtility.TryDevelopBondRelation(doctor, patient, 0.004f) && doctor.Faction != null && doctor.Faction != patient.Faction)
		{
			InteractionWorker_RecruitAttempt.DoRecruit(doctor, patient, useAudiovisualEffects: false);
		}
		patient.records.Increment(RecordDefOf.TimesTendedTo);
		doctor?.records.Increment(RecordDefOf.TimesTendedOther);
		if (doctor == patient && !doctor.Dead)
		{
			doctor.mindState.Notify_SelfTended();
		}
		if (medicine != null)
		{
			if ((patient.Spawned || (doctor != null && doctor.Spawned)) && medicine.GetStatValue(StatDefOf.MedicalPotency) > ThingDefOf.MedicineIndustrial.GetStatValueAbstract(StatDefOf.MedicalPotency))
			{
				SoundDefOf.TechMedicineUsed.PlayOneShot(new TargetInfo(patient.Position, patient.Map));
			}
			if (medicine.stackCount > 1)
			{
				medicine.stackCount--;
			}
			else if (!medicine.Destroyed)
			{
				medicine.Destroy();
			}
		}
		if (ModsConfig.IdeologyActive && doctor?.Ideo != null)
		{
			Precept_Role role = doctor.Ideo.GetRole(doctor);
			if (role?.def.roleEffects != null)
			{
				foreach (RoleEffect roleEffect in role.def.roleEffects)
				{
					roleEffect.Notify_Tended(doctor, patient);
				}
			}
		}
		if (doctor != null && doctor.Faction == Faction.OfPlayer && doctor != patient)
		{
			QuestUtility.SendQuestTargetSignals(patient.questTags, "PlayerTended", patient.Named("SUBJECT"));
		}
	}

	public static float CalculateBaseTendQuality(Pawn doctor, Pawn patient, ThingDef medicine)
	{
		float medicinePotency = medicine?.GetStatValueAbstract(StatDefOf.MedicalPotency) ?? 0.3f;
		float medicineQualityMax = medicine?.GetStatValueAbstract(StatDefOf.MedicalQualityMax) ?? 0.7f;
		return CalculateBaseTendQuality(doctor, patient, medicinePotency, medicineQualityMax);
	}

	public static float CalculateBaseTendQuality(Pawn doctor, Pawn patient, float medicinePotency, float medicineQualityMax)
	{
		float num = doctor?.GetStatValue(StatDefOf.MedicalTendQuality) ?? 0.75f;
		num *= medicinePotency;
		Building_Bed building_Bed = patient?.CurrentBed();
		if (building_Bed != null)
		{
			num += building_Bed.GetStatValue(StatDefOf.MedicalTendQualityOffset);
		}
		if (doctor == patient && doctor != null)
		{
			num *= 0.7f;
		}
		return Mathf.Clamp(num, 0f, medicineQualityMax);
	}

	public static void GetOptimalHediffsToTendWithSingleTreatment(Pawn patient, bool usingMedicine, List<Hediff> outHediffsToTend, List<Hediff> tendableHediffsInTendPriorityOrder = null)
	{
		outHediffsToTend.Clear();
		tmpHediffs.Clear();
		if (tendableHediffsInTendPriorityOrder != null)
		{
			tmpHediffs.AddRange(tendableHediffsInTendPriorityOrder);
		}
		else
		{
			List<Hediff> hediffs = patient.health.hediffSet.hediffs;
			for (int i = 0; i < hediffs.Count; i++)
			{
				if (hediffs[i].TendableNow())
				{
					tmpHediffs.Add(hediffs[i]);
				}
			}
			SortByTendPriority(tmpHediffs);
		}
		if (!tmpHediffs.Any())
		{
			return;
		}
		Hediff hediff = tmpHediffs[0];
		outHediffsToTend.Add(hediff);
		HediffCompProperties_TendDuration hediffCompProperties_TendDuration = hediff.def.CompProps<HediffCompProperties_TendDuration>();
		if (hediffCompProperties_TendDuration != null && hediffCompProperties_TendDuration.tendAllAtOnce)
		{
			for (int j = 0; j < tmpHediffs.Count; j++)
			{
				if (tmpHediffs[j] != hediff && tmpHediffs[j].def == hediff.def)
				{
					outHediffsToTend.Add(tmpHediffs[j]);
				}
			}
		}
		else if (hediff is Hediff_Injury && usingMedicine)
		{
			float num = hediff.Severity;
			for (int k = 0; k < tmpHediffs.Count; k++)
			{
				if (tmpHediffs[k] != hediff && tmpHediffs[k] is Hediff_Injury { Severity: var severity } hediff_Injury && num + severity <= 20f)
				{
					num += severity;
					outHediffsToTend.Add(hediff_Injury);
				}
			}
		}
		tmpHediffs.Clear();
	}

	public static void SortByTendPriority(List<Hediff> hediffs)
	{
		if (hediffs.Count > 1)
		{
			tmpHediffsWithTendPriority.Clear();
			for (int i = 0; i < hediffs.Count; i++)
			{
				tmpHediffsWithTendPriority.Add(new Pair<Hediff, float>(hediffs[i], hediffs[i].TendPriority));
			}
			tmpHediffsWithTendPriority.SortByDescending((Pair<Hediff, float> x) => x.Second, (Pair<Hediff, float> x) => x.First.Severity);
			hediffs.Clear();
			for (int num = 0; num < tmpHediffsWithTendPriority.Count; num++)
			{
				hediffs.Add(tmpHediffsWithTendPriority[num].First);
			}
			tmpHediffsWithTendPriority.Clear();
		}
	}
}
