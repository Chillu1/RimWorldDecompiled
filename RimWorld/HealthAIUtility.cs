using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class HealthAIUtility
	{
		public static bool ShouldSeekMedicalRestUrgent(Pawn pawn)
		{
			if (!pawn.Downed && !pawn.health.HasHediffsNeedingTend())
			{
				return ShouldHaveSurgeryDoneNow(pawn);
			}
			return true;
		}

		public static bool ShouldSeekMedicalRest(Pawn pawn)
		{
			if (!ShouldSeekMedicalRestUrgent(pawn) && !pawn.health.hediffSet.HasTendedAndHealingInjury())
			{
				return pawn.health.hediffSet.HasImmunizableNotImmuneHediff();
			}
			return true;
		}

		public static bool ShouldBeTendedNowByPlayerUrgent(Pawn pawn)
		{
			if (ShouldBeTendedNowByPlayer(pawn))
			{
				return HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) < 45000;
			}
			return false;
		}

		public static bool ShouldBeTendedNowByPlayer(Pawn pawn)
		{
			if (pawn.playerSettings == null)
			{
				return false;
			}
			if (!ShouldEverReceiveMedicalCareFromPlayer(pawn))
			{
				return false;
			}
			return pawn.health.HasHediffsNeedingTendByPlayer();
		}

		public static bool ShouldEverReceiveMedicalCareFromPlayer(Pawn pawn)
		{
			if (pawn.playerSettings != null && pawn.playerSettings.medCare == MedicalCareCategory.NoCare)
			{
				return false;
			}
			if (pawn.guest != null && pawn.guest.interactionMode == PrisonerInteractionModeDefOf.Execution)
			{
				return false;
			}
			if (pawn.Map?.designationManager.DesignationOn(pawn, DesignationDefOf.Slaughter) != null)
			{
				return false;
			}
			return true;
		}

		public static bool ShouldHaveSurgeryDoneNow(Pawn pawn)
		{
			return pawn.health.surgeryBills.AnyShouldDoNow;
		}

		public static Thing FindBestMedicine(Pawn healer, Pawn patient)
		{
			if (patient.playerSettings == null || (int)patient.playerSettings.medCare <= 1)
			{
				return null;
			}
			if (Medicine.GetMedicineCountToFullyHeal(patient) <= 0)
			{
				return null;
			}
			Predicate<Thing> validator = (Thing m) => (!m.IsForbidden(healer) && patient.playerSettings.medCare.AllowsMedicine(m.def) && healer.CanReserve(m, 10, 1)) ? true : false;
			Func<Thing, float> priorityGetter = (Thing t) => t.def.GetStatValueAbstract(StatDefOf.MedicalPotency);
			return GenClosest.ClosestThing_Global_Reachable(patient.Position, patient.Map, patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine), PathEndMode.ClosestTouch, TraverseParms.For(healer), 9999f, validator, priorityGetter);
		}
	}
}
