using System;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class HealthAIUtility
{
	private const float MinDistFromEnemyToRescue = 25f;

	public static bool ShouldSeekMedicalRestUrgent(Pawn pawn)
	{
		if ((!pawn.Downed || LifeStageUtility.AlwaysDowned(pawn)) && !pawn.health.HasHediffsNeedingTend() && !ShouldHaveSurgeryDoneNow(pawn))
		{
			return pawn.health.hediffSet.InLabor();
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
		Pawn_PlayerSettings playerSettings = pawn.playerSettings;
		if (playerSettings != null && playerSettings.medCare == MedicalCareCategory.NoCare)
		{
			return false;
		}
		if (pawn.guest != null && pawn.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Execution))
		{
			return false;
		}
		if (pawn.ShouldBeSlaughtered())
		{
			return false;
		}
		return true;
	}

	public static bool ShouldHaveSurgeryDoneNow(Pawn pawn)
	{
		if (pawn.health.surgeryBills.AnyShouldDoNow)
		{
			return WorkGiver_PatientGoToBedTreatment.AnyAvailableDoctorFor(pawn);
		}
		return false;
	}

	public static bool WantsToBeRescued(Pawn pawn)
	{
		if (!pawn.Downed)
		{
			return false;
		}
		if (pawn.InBed() || pawn.IsCharging())
		{
			return false;
		}
		if (pawn.IsDeactivated())
		{
			return false;
		}
		if (CaravanFormingUtility.IsFormingCaravanOrDownedPawnToBeTakenByCaravan(pawn))
		{
			return false;
		}
		if (pawn.IsMutant && !pawn.mutant.Def.entitledToMedicalCare)
		{
			return false;
		}
		if (pawn.InMentalState && !pawn.health.hediffSet.HasHediff(HediffDefOf.Scaria))
		{
			return false;
		}
		if (pawn.ShouldBeSlaughtered())
		{
			return false;
		}
		if (pawn.TryGetLord(out var lord) && lord.LordJob is LordJob_Ritual lordJob_Ritual && lordJob_Ritual.TryGetRoleFor(pawn, out var role) && role.allowDowned)
		{
			return false;
		}
		if (LifeStageUtility.AlwaysDowned(pawn))
		{
			return ShouldSeekMedicalRest(pawn);
		}
		return true;
	}

	public static bool CanRescueNow(Pawn rescuer, Pawn patient, bool forced = false)
	{
		if (!forced && patient.Faction != rescuer.Faction)
		{
			return false;
		}
		if (!WantsToBeRescued(patient))
		{
			return false;
		}
		if (!forced && patient.IsForbidden(rescuer))
		{
			return false;
		}
		if (!forced && GenAI.EnemyIsNear(patient, 25f))
		{
			return false;
		}
		if (!rescuer.CanReserveAndReach(patient, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}

	public static Thing FindBestMedicine(Pawn healer, Pawn patient, bool onlyUseInventory = false)
	{
		if (patient.playerSettings != null && (int)patient.playerSettings.medCare <= 1)
		{
			return null;
		}
		if (Medicine.GetMedicineCountToFullyHeal(patient) <= 0)
		{
			return null;
		}
		Predicate<Thing> validator = delegate(Thing m)
		{
			bool flag = ((patient.playerSettings == null) ? MedicalCareCategory.NoMeds : patient.playerSettings.medCare).AllowsMedicine(m.def);
			if (patient.playerSettings == null && onlyUseInventory)
			{
				flag = true;
			}
			return (!m.IsForbidden(healer) && flag && healer.CanReserve(m, 10, 1)) ? true : false;
		};
		Thing thing = GetBestMedInInventory(healer.inventory.innerContainer);
		if (onlyUseInventory)
		{
			return thing;
		}
		Thing thing2 = GenClosest.ClosestThing_Global_Reachable(patient.PositionHeld, patient.MapHeld, patient.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Medicine), PathEndMode.ClosestTouch, TraverseParms.For(healer), 9999f, validator, PriorityOf);
		if (thing != null && thing2 != null)
		{
			if (!(PriorityOf(thing) >= PriorityOf(thing2)))
			{
				return thing2;
			}
			return thing;
		}
		if (thing == null && thing2 == null && healer.IsColonist && healer.Map != null)
		{
			Thing thing3 = null;
			foreach (Pawn spawnedColonyAnimal in healer.Map.mapPawns.SpawnedColonyAnimals)
			{
				thing3 = GetBestMedInInventory(spawnedColonyAnimal.inventory.innerContainer);
				if (thing3 != null && (thing2 == null || PriorityOf(thing2) < PriorityOf(thing3)) && !spawnedColonyAnimal.IsForbidden(healer) && healer.CanReach(spawnedColonyAnimal, PathEndMode.OnCell, Danger.Some))
				{
					thing2 = thing3;
				}
			}
		}
		return thing ?? thing2;
		Thing GetBestMedInInventory(ThingOwner inventory)
		{
			if (inventory.Count == 0)
			{
				return null;
			}
			return inventory.Where((Thing t) => t.def.IsMedicine && validator(t)).OrderByDescending(PriorityOf).FirstOrDefault();
		}
		static float PriorityOf(Thing t)
		{
			return t.def.GetStatValueAbstract(StatDefOf.MedicalPotency);
		}
	}
}
