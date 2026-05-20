using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class RestUtility
{
	public const int NoSleepingDurationAfterBeingDisturbed = 400;

	private static List<ThingDef> bedDefsBestToWorst_RestEffectiveness;

	private static List<ThingDef> bedDefsBestToWorst_Medical;

	private static List<ThingDef> bedDefsBestToWorst_SlabBed_RestEffectiveness;

	private static List<ThingDef> bedDefsBestToWorst_SlabBed_Medical;

	public static List<ThingDef> AllBedDefBestToWorst => bedDefsBestToWorst_RestEffectiveness;

	public static void Reset()
	{
		bedDefsBestToWorst_RestEffectiveness = (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsBed
			orderby d.building.bed_maxBodySize, d.GetStatValueAbstract(StatDefOf.BedRestEffectiveness) descending
			select d).ToList();
		bedDefsBestToWorst_SlabBed_RestEffectiveness = (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsBed
			orderby (!d.building.bed_slabBed) ? 1 : 0, d.building.bed_maxBodySize, d.GetStatValueAbstract(StatDefOf.BedRestEffectiveness) descending
			select d).ToList();
		bedDefsBestToWorst_Medical = (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsBed
			orderby d.building.bed_maxBodySize, d.GetStatValueAbstract(StatDefOf.MedicalTendQualityOffset) descending, d.GetStatValueAbstract(StatDefOf.BedRestEffectiveness) descending
			select d).ToList();
		bedDefsBestToWorst_SlabBed_Medical = (from d in DefDatabase<ThingDef>.AllDefs
			where d.IsBed
			orderby (!d.building.bed_slabBed) ? 1 : 0, d.building.bed_maxBodySize, d.GetStatValueAbstract(StatDefOf.MedicalTendQualityOffset) descending, d.GetStatValueAbstract(StatDefOf.BedRestEffectiveness) descending
			select d).ToList();
	}

	public static bool BedOwnerWillShare(Building_Bed bed, Pawn sleeper, GuestStatus? guestStatus)
	{
		if (!bed.OwnersForReading.Any())
		{
			return true;
		}
		if (sleeper.IsPrisoner || guestStatus == GuestStatus.Prisoner || sleeper.IsSlave || guestStatus == GuestStatus.Slave)
		{
			if (!bed.AnyUnownedSleepingSlot)
			{
				return false;
			}
		}
		else
		{
			if (!bed.AnyUnownedSleepingSlot)
			{
				return false;
			}
			if (!IsAnyOwnerLovePartnerOf(bed, sleeper))
			{
				return false;
			}
		}
		return true;
	}

	public static bool CanUseBedNow(Thing bedThing, Pawn sleeper, bool checkSocialProperness, bool allowMedBedEvenIfSetToNoCare = false, GuestStatus? guestStatusOverride = null)
	{
		if (!(bedThing is Building_Bed building_Bed))
		{
			return false;
		}
		if (!building_Bed.Spawned)
		{
			return false;
		}
		if (building_Bed.Map != sleeper.MapHeld)
		{
			return false;
		}
		if (building_Bed.IsBurning())
		{
			return false;
		}
		if (sleeper.HarmedByVacuum && building_Bed.Position.GetVacuum(bedThing.Map) >= 0.5f)
		{
			return false;
		}
		if (!CanUseBedEver(sleeper, building_Bed.def))
		{
			return false;
		}
		if (building_Bed.CompAssignableToPawn.IdeoligionForbids(sleeper))
		{
			return false;
		}
		int? assignedSleepingSlot;
		bool flag = building_Bed.IsOwner(sleeper, out assignedSleepingSlot);
		int? sleepingSlot;
		bool flag2 = sleeper.CurrentBed(out sleepingSlot) == building_Bed;
		if (!building_Bed.AnyUnoccupiedSleepingSlot && !flag && !flag2)
		{
			return false;
		}
		GuestStatus? obj = guestStatusOverride ?? sleeper.GuestStatus;
		bool flag3 = obj == GuestStatus.Prisoner;
		bool flag4 = obj == GuestStatus.Slave;
		if (checkSocialProperness && !building_Bed.IsSociallyProper(sleeper, flag3))
		{
			return false;
		}
		if (building_Bed.ForPrisoners != flag3)
		{
			return false;
		}
		if (building_Bed.ForSlaves != flag4)
		{
			return false;
		}
		if (building_Bed.ForPrisoners && !building_Bed.Position.IsInPrisonCell(building_Bed.Map))
		{
			return false;
		}
		if (building_Bed.Medical)
		{
			if (!allowMedBedEvenIfSetToNoCare && !HealthAIUtility.ShouldEverReceiveMedicalCareFromPlayer(sleeper))
			{
				return false;
			}
			if (!HealthAIUtility.ShouldSeekMedicalRest(sleeper))
			{
				return false;
			}
		}
		else
		{
			if (!flag && !BedOwnerWillShare(building_Bed, sleeper, guestStatusOverride))
			{
				return false;
			}
			if (flag2 && sleepingSlot != assignedSleepingSlot)
			{
				return false;
			}
		}
		if (sleeper.IsColonist && !flag3)
		{
			Job curJob = sleeper.CurJob;
			if ((curJob == null || !curJob.ignoreForbidden) && !sleeper.Downed && building_Bed.IsForbidden(sleeper))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsValidBedFor(Thing bedThing, Pawn sleeper, Pawn traveler, bool checkSocialProperness, bool allowMedBedEvenIfSetToNoCare = false, bool ignoreOtherReservations = false, GuestStatus? guestStatus = null)
	{
		if (!CanUseBedNow(bedThing, sleeper, checkSocialProperness, allowMedBedEvenIfSetToNoCare, guestStatus))
		{
			return false;
		}
		Building_Bed building_Bed = (Building_Bed)bedThing;
		if (!traveler.CanReach(building_Bed, PathEndMode.OnCell, Danger.Some))
		{
			return false;
		}
		if (!sleeper.HasReserved(building_Bed) && !traveler.CanReserve(building_Bed, building_Bed.SleepingSlotsCount, 0, null, ignoreOtherReservations))
		{
			return false;
		}
		if (traveler.HasReserved<JobDriver_TakeToBed>(building_Bed, sleeper))
		{
			return false;
		}
		if (building_Bed.IsForbidden(traveler))
		{
			return false;
		}
		bool num = guestStatus == GuestStatus.Prisoner;
		bool flag = guestStatus == GuestStatus.Slave;
		if (!num && !flag && building_Bed.Faction != traveler.Faction && (traveler.HostFaction == null || building_Bed.Faction != traveler.HostFaction))
		{
			return false;
		}
		if (ModsConfig.AnomalyActive && sleeper.IsMutant && sleeper.needs.rest == null && sleeper.mutant.Def.entitledToMedicalCare && !building_Bed.Medical)
		{
			return false;
		}
		return true;
	}

	public static void TuckIntoBed(Building_Bed bed, Pawn taker, Pawn takee, bool rescued)
	{
		IntVec3 position = bed.Position;
		if (taker != takee)
		{
			taker.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out var _);
		}
		if (CanUseBedNow(bed, takee, checkSocialProperness: false))
		{
			takee.jobs.Notify_TuckedIntoBed(bed);
			if (taker != takee && rescued)
			{
				takee.relations.Notify_RescuedBy(taker);
			}
			takee.mindState.Notify_TuckedIntoBed();
		}
		if (takee.IsPrisonerOfColony)
		{
			LessonAutoActivator.TeachOpportunity(ConceptDefOf.PrisonerTab, takee, OpportunityType.GoodToKnow);
		}
	}

	private static bool IsAnyOwnerLovePartnerOf(Building_Bed bed, Pawn sleeper)
	{
		for (int i = 0; i < bed.OwnersForReading.Count; i++)
		{
			if (LovePartnerRelationUtility.LovePartnerRelationExists(sleeper, bed.OwnersForReading[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static Building_Bed FindBedFor(Pawn p)
	{
		return FindBedFor(p, p, checkSocialProperness: true, ignoreOtherReservations: false, p.GuestStatus);
	}

	public static Building_Bed FindBedFor(Pawn sleeper, Pawn traveler, bool checkSocialProperness, bool ignoreOtherReservations = false, GuestStatus? guestStatus = null)
	{
		if (sleeper.RaceProps.IsMechanoid)
		{
			return null;
		}
		if (ModsConfig.BiotechActive && sleeper.Deathresting)
		{
			Building_Bed assignedDeathrestCasket = sleeper.ownership.AssignedDeathrestCasket;
			if (assignedDeathrestCasket != null && IsValidBedFor(assignedDeathrestCasket, sleeper, traveler, checkSocialProperness: true))
			{
				CompDeathrestBindable compDeathrestBindable = assignedDeathrestCasket.TryGetComp<CompDeathrestBindable>();
				if (compDeathrestBindable != null && (compDeathrestBindable.BoundPawn == sleeper || compDeathrestBindable.BoundPawn == null))
				{
					return assignedDeathrestCasket;
				}
			}
		}
		bool flag = false;
		if (sleeper.Ideo != null)
		{
			foreach (Precept item in sleeper.Ideo.PreceptsListForReading)
			{
				if (item.def.prefersSlabBed)
				{
					flag = true;
					break;
				}
			}
		}
		List<ThingDef> list = (flag ? bedDefsBestToWorst_SlabBed_Medical : bedDefsBestToWorst_Medical);
		List<ThingDef> list2 = (flag ? bedDefsBestToWorst_SlabBed_RestEffectiveness : bedDefsBestToWorst_RestEffectiveness);
		if (HealthAIUtility.ShouldSeekMedicalRest(sleeper))
		{
			if (sleeper.InBed() && sleeper.CurrentBed().Medical && IsValidBedFor(sleeper.CurrentBed(), sleeper, traveler, checkSocialProperness, allowMedBedEvenIfSetToNoCare: false, ignoreOtherReservations, guestStatus))
			{
				return sleeper.CurrentBed();
			}
			for (int i = 0; i < list.Count; i++)
			{
				ThingDef thingDef = list[i];
				if (!CanUseBedEver(sleeper, thingDef))
				{
					continue;
				}
				for (int j = 0; j < 2; j++)
				{
					Danger maxDanger = ((j == 0) ? Danger.None : Danger.Deadly);
					Building_Bed building_Bed = (Building_Bed)GenClosest.ClosestThingReachable(sleeper.Position, sleeper.MapHeld, ThingRequest.ForDef(thingDef), PathEndMode.OnCell, TraverseParms.For(traveler), 9999f, (Thing b) => ((Building_Bed)b).Medical && (int)b.Position.GetDangerFor(sleeper, sleeper.Map) <= (int)maxDanger && IsValidBedFor(b, sleeper, traveler, checkSocialProperness, allowMedBedEvenIfSetToNoCare: false, ignoreOtherReservations, guestStatus));
					if (building_Bed != null)
					{
						return building_Bed;
					}
				}
			}
		}
		if (sleeper.RaceProps.Dryad)
		{
			return null;
		}
		if (sleeper.ownership != null && sleeper.ownership.OwnedBed != null && IsValidBedFor(sleeper.ownership.OwnedBed, sleeper, traveler, checkSocialProperness, allowMedBedEvenIfSetToNoCare: false, ignoreOtherReservations, guestStatus))
		{
			return sleeper.ownership.OwnedBed;
		}
		DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingMostLikedLovePartnerRel(sleeper, allowDead: false);
		if (directPawnRelation != null)
		{
			Building_Bed ownedBed = directPawnRelation.otherPawn.ownership.OwnedBed;
			if (ownedBed != null && IsValidBedFor(ownedBed, sleeper, traveler, checkSocialProperness, allowMedBedEvenIfSetToNoCare: false, ignoreOtherReservations, guestStatus))
			{
				return ownedBed;
			}
		}
		for (int dg = 0; dg < 3; dg++)
		{
			Danger maxDanger2 = ((dg <= 1) ? Danger.None : Danger.Deadly);
			for (int num = 0; num < list2.Count; num++)
			{
				ThingDef thingDef2 = list2[num];
				if (!CanUseBedEver(sleeper, thingDef2))
				{
					continue;
				}
				Building_Bed building_Bed2 = (Building_Bed)GenClosest.ClosestThingReachable(sleeper.PositionHeld, sleeper.MapHeld, ThingRequest.ForDef(thingDef2), PathEndMode.OnCell, TraverseParms.For(traveler), 9999f, (Thing b) => !((Building_Bed)b).Medical && (int)b.Position.GetDangerFor(sleeper, sleeper.MapHeld) <= (int)maxDanger2 && IsValidBedFor(b, sleeper, traveler, checkSocialProperness, allowMedBedEvenIfSetToNoCare: false, ignoreOtherReservations, guestStatus) && (dg > 0 || !b.Position.GetItems(b.Map).Any((Thing thing) => thing.def.IsCorpse)));
				if (building_Bed2 != null)
				{
					return building_Bed2;
				}
			}
		}
		return null;
	}

	public static Building_Bed FindPatientBedFor(Pawn pawn)
	{
		Predicate<Thing> medBedValidator = delegate(Thing t)
		{
			if (!(t is Building_Bed building_Bed2))
			{
				return false;
			}
			if (!building_Bed2.Medical)
			{
				return false;
			}
			return IsValidBedFor(building_Bed2, pawn, pawn, checkSocialProperness: false, allowMedBedEvenIfSetToNoCare: true, ignoreOtherReservations: false, pawn.GuestStatus) ? true : false;
		};
		if (pawn.InBed() && medBedValidator(pawn.CurrentBed()))
		{
			return pawn.CurrentBed();
		}
		for (int num = 0; num < 2; num++)
		{
			Danger maxDanger = ((num == 0) ? Danger.None : Danger.Deadly);
			Building_Bed building_Bed = (Building_Bed)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Bed), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f, (Thing b) => (int)b.Position.GetDangerFor(pawn, pawn.Map) <= (int)maxDanger && medBedValidator(b));
			if (building_Bed != null)
			{
				return building_Bed;
			}
		}
		return FindBedFor(pawn);
	}

	public static IntVec3 GetBedSleepingSlotPosFor(Pawn pawn, Building_Bed bed)
	{
		if (bed.IsOwner(pawn, out var assignedSleepingSlot))
		{
			return bed.GetSleepingSlotPos(assignedSleepingSlot.Value);
		}
		for (int i = 0; i < bed.SleepingSlotsCount; i++)
		{
			Pawn curOccupant = bed.GetCurOccupant(i);
			if ((i >= bed.OwnersForReading.Count || bed.OwnersForReading[i] == null) && curOccupant == pawn)
			{
				return bed.GetSleepingSlotPos(i);
			}
		}
		for (int j = 0; j < bed.SleepingSlotsCount; j++)
		{
			Pawn curOccupant2 = bed.GetCurOccupant(j);
			if ((j >= bed.OwnersForReading.Count || bed.OwnersForReading[j] == null) && curOccupant2 == null)
			{
				return bed.GetSleepingSlotPos(j);
			}
		}
		Log.Error("Could not find good sleeping slot position for " + pawn?.ToString() + ". Perhaps AnyUnoccupiedSleepingSlot check is missing somewhere.");
		return bed.GetSleepingSlotPos(0);
	}

	public static void KickOutOfBed(Pawn pawn, Building_Bed bed)
	{
		if (pawn == null)
		{
			return;
		}
		if (!pawn.Spawned)
		{
			Log.Error("Tried to kick unspawned pawn " + pawn.ToStringSafe() + " out of bed.");
		}
		if (!pawn.Dead && !pawn.GetPosture().InBed())
		{
			Log.Error("Tried to kick pawn " + pawn.ToStringSafe() + " out of bed when they weren't in bed.");
		}
		int? sleepingSlot;
		Building_Bed building_Bed = pawn.CurrentBed(out sleepingSlot);
		if (building_Bed != bed)
		{
			if (building_Bed == null)
			{
				bed = null;
			}
			else
			{
				Log.Error("Tried to kick pawn " + pawn.ToStringSafe() + " out of a bed they're not currently in.");
			}
		}
		pawn.jobs.posture &= ~PawnPosture.InBedMask;
		if (bed != null && (pawn.Downed || pawn.Deathresting))
		{
			pawn.Position = bed.GetFootSlotPos(sleepingSlot.Value);
		}
	}

	public static bool CanUseBedEver(Pawn p, ThingDef bedDef)
	{
		if (p.RaceProps.IsMechanoid)
		{
			return false;
		}
		if (p.BodySize > bedDef.building.bed_maxBodySize)
		{
			return false;
		}
		if (p.RaceProps.Humanlike != bedDef.building.bed_humanlike)
		{
			return false;
		}
		if (ModsConfig.BiotechActive && bedDef == ThingDefOf.DeathrestCasket && !p.CanDeathrest())
		{
			return false;
		}
		return true;
	}

	public static bool TimetablePreventsLayDown(Pawn pawn)
	{
		if (pawn.timetable?.CurrentAssignment != null && !pawn.timetable.CurrentAssignment.allowRest && pawn.needs?.rest != null && pawn.needs.rest.CurLevel >= 0.2f)
		{
			return true;
		}
		return false;
	}

	public static bool DisturbancePreventsLyingDown(Pawn pawn)
	{
		if (pawn.Downed)
		{
			return false;
		}
		return Find.TickManager.TicksGame - pawn.mindState.lastDisturbanceTick < 400;
	}

	public static bool Awake(this Pawn p)
	{
		if (!p.health.capacities.CanBeAwake)
		{
			return false;
		}
		if (p.CurJob != null && p.jobs.curDriver != null)
		{
			return !p.jobs.curDriver.asleep;
		}
		return true;
	}

	public static bool IsSelfShutdown(this Pawn p)
	{
		if (p.needs?.energy == null)
		{
			return false;
		}
		return p.needs.energy.IsSelfShutdown;
	}

	public static bool IsDeactivated(this Pawn p)
	{
		return p.TryGetComp<CompMechanoid>()?.Deactivated ?? false;
	}

	public static bool IsActivityDormant(this Pawn p)
	{
		return p.activity?.IsActive ?? false;
	}

	public static bool IsCharging(this Pawn p)
	{
		return p.needs?.energy?.currentCharger != null;
	}

	public static Building_Bed CurrentBed(this Pawn p)
	{
		int? sleepingSlot;
		return p.CurrentBed(out sleepingSlot);
	}

	public static Building_Bed CurrentBed(this Pawn p, out int? sleepingSlot)
	{
		sleepingSlot = null;
		if (!p.Spawned || p.CurJob == null || !p.GetPosture().InBed())
		{
			return null;
		}
		Building_Bed building_Bed = null;
		List<Thing> thingList = p.Position.GetThingList(p.Map);
		for (int i = 0; i < thingList.Count; i++)
		{
			building_Bed = thingList[i] as Building_Bed;
			if (building_Bed != null)
			{
				break;
			}
		}
		if (building_Bed == null)
		{
			return null;
		}
		for (int j = 0; j < building_Bed.SleepingSlotsCount; j++)
		{
			if (building_Bed.GetCurOccupant(j) == p)
			{
				sleepingSlot = j;
				return building_Bed;
			}
		}
		return null;
	}

	public static bool InBed(this Pawn p)
	{
		return p.CurrentBed() != null;
	}

	public static bool IsLayingForJobCleanup(Pawn p)
	{
		if (!p.InBed())
		{
			if (p.CurJob != null && p.CurJob.def == JobDefOf.LayDown)
			{
				return p.GetPosture().Laying();
			}
			return false;
		}
		return true;
	}

	public static void WakeUp(Pawn p, bool startNewJob = true)
	{
		if (p.CurJob != null && (p.GetPosture().Laying() || p.CurJobDef == JobDefOf.LayDown) && !p.Downed)
		{
			p.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob);
		}
		p.GetComp<CompCanBeDormant>()?.WakeUp();
		if (p.mindState != null)
		{
			p.mindState.hibernationEndedTick = GenTicks.TicksGame;
		}
	}

	public static bool ShouldWakeUp(Pawn pawn)
	{
		if (pawn.Deathresting)
		{
			return false;
		}
		if (pawn.needs?.rest != null && !(pawn.needs.rest.CurLevel >= WakeThreshold(pawn)))
		{
			return pawn.health.hediffSet.HasHediffBlocksSleeping();
		}
		return true;
	}

	public static bool CanFallAsleep(Pawn pawn)
	{
		if (pawn.Deathresting)
		{
			return true;
		}
		Pawn_NeedsTracker needs = pawn.needs;
		if (needs != null && needs.food?.Starving == true && !pawn.ageTracker.CurLifeStage.canSleepWhenStarving)
		{
			return false;
		}
		if (pawn.mindState != null && Find.TickManager.TicksGame - pawn.mindState.lastDisturbanceTick < 400)
		{
			return false;
		}
		if (pawn.needs?.rest?.CurLevel < FallAsleepMaxLevel(pawn) && !pawn.health.hediffSet.HasHediffBlocksSleeping())
		{
			return pawn.CurJobDef?.sleepCanInterrupt ?? true;
		}
		return false;
	}

	private static float WakeThreshold(Pawn p)
	{
		Lord lord = p.GetLord();
		if (lord != null && lord.CurLordToil != null && lord.CurLordToil.CustomWakeThreshold.HasValue)
		{
			return lord.CurLordToil.CustomWakeThreshold.Value;
		}
		return p.ageTracker.CurLifeStage?.naturalWakeThresholdOverride ?? 1f;
	}

	private static float FallAsleepMaxLevel(Pawn p)
	{
		return Mathf.Min(p.ageTracker.CurLifeStage?.fallAsleepMaxThresholdOverride ?? 0.75f, WakeThreshold(p) - 0.01f);
	}
}
