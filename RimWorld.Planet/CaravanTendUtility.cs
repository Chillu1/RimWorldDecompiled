using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanTendUtility
	{
		private static List<Pawn> tmpPawnsNeedingTreatment = new List<Pawn>();

		private const int TendIntervalTicks = 1250;

		public static void CheckTend(Caravan caravan)
		{
			for (int i = 0; i < caravan.pawns.Count; i++)
			{
				Pawn pawn = caravan.pawns[i];
				if (IsValidDoctorFor(pawn, null, caravan) && pawn.IsHashIntervalTick(1250))
				{
					TryTendToAnyPawn(caravan);
				}
			}
		}

		public static void TryTendToAnyPawn(Caravan caravan)
		{
			FindPawnsNeedingTend(caravan, tmpPawnsNeedingTreatment);
			if (!tmpPawnsNeedingTreatment.Any())
			{
				return;
			}
			tmpPawnsNeedingTreatment.SortByDescending((Pawn x) => GetTendPriority(x));
			Pawn patient = null;
			Pawn pawn = null;
			for (int i = 0; i < tmpPawnsNeedingTreatment.Count; i++)
			{
				patient = tmpPawnsNeedingTreatment[i];
				pawn = FindBestDoctorFor(caravan, patient);
				if (pawn != null)
				{
					break;
				}
			}
			if (pawn != null)
			{
				Medicine medicine = null;
				Pawn owner = null;
				CaravanInventoryUtility.TryGetBestMedicine(caravan, patient, out medicine, out owner);
				TendUtility.DoTend(pawn, patient, medicine);
				if (medicine != null && medicine.Destroyed)
				{
					owner?.inventory.innerContainer.Remove(medicine);
				}
				tmpPawnsNeedingTreatment.Clear();
			}
		}

		private static void FindPawnsNeedingTend(Caravan caravan, List<Pawn> outPawnsNeedingTend)
		{
			outPawnsNeedingTend.Clear();
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Pawn pawn = pawnsListForReading[i];
				if ((pawn.playerSettings == null || (int)pawn.playerSettings.medCare > 0) && pawn.health.HasHediffsNeedingTend())
				{
					outPawnsNeedingTend.Add(pawn);
				}
			}
		}

		private static Pawn FindBestDoctorFor(Caravan caravan, Pawn patient)
		{
			float num = 0f;
			Pawn pawn = null;
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Pawn pawn2 = pawnsListForReading[i];
				if (IsValidDoctorFor(pawn2, patient, caravan))
				{
					float statValue = pawn2.GetStatValue(StatDefOf.MedicalTendQuality);
					if (statValue > num || pawn == null)
					{
						num = statValue;
						pawn = pawn2;
					}
				}
			}
			return pawn;
		}

		private static bool IsValidDoctorFor(Pawn doctor, Pawn patient, Caravan caravan)
		{
			if (!doctor.RaceProps.Humanlike)
			{
				return false;
			}
			if (!caravan.IsOwner(doctor))
			{
				return false;
			}
			if (doctor == patient && (!doctor.IsColonist || !doctor.playerSettings.selfTend))
			{
				return false;
			}
			if (doctor.Downed || doctor.InMentalState)
			{
				return false;
			}
			if (doctor.story != null && doctor.WorkTypeIsDisabled(WorkTypeDefOf.Doctor))
			{
				return false;
			}
			return true;
		}

		private static float GetTendPriority(Pawn patient)
		{
			int num = HealthUtility.TicksUntilDeathDueToBloodLoss(patient);
			if (num < 15000)
			{
				if (patient.RaceProps.Humanlike)
				{
					return GenMath.LerpDouble(0f, 15000f, 5f, 4f, num);
				}
				return GenMath.LerpDouble(0f, 15000f, 4f, 3f, num);
			}
			for (int i = 0; i < patient.health.hediffSet.hediffs.Count; i++)
			{
				Hediff hediff = patient.health.hediffSet.hediffs[i];
				HediffStage curStage = hediff.CurStage;
				if (((curStage != null && curStage.lifeThreatening) || hediff.def.lethalSeverity >= 0f) && hediff.TendableNow())
				{
					if (patient.RaceProps.Humanlike)
					{
						return 2.5f;
					}
					return 2f;
				}
			}
			if (patient.health.hediffSet.BleedRateTotal >= 0.0001f)
			{
				if (patient.RaceProps.Humanlike)
				{
					return 1.5f;
				}
				return 1f;
			}
			if (patient.RaceProps.Humanlike)
			{
				return 0.5f;
			}
			return 0f;
		}
	}
}
