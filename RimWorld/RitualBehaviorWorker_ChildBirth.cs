using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class RitualBehaviorWorker_ChildBirth : RitualBehaviorWorker
{
	public override bool ChecksReservations => false;

	public override string descriptionOverride
	{
		get
		{
			if (Find.Storyteller.difficulty.babiesAreHealthy)
			{
				return "ChildBirthAlwaysHealthy_Desc".Translate();
			}
			return null;
		}
	}

	public RitualBehaviorWorker_ChildBirth()
	{
	}

	public RitualBehaviorWorker_ChildBirth(RitualBehaviorDef def)
		: base(def)
	{
	}

	public override void Tick(LordJob_Ritual ritual)
	{
		Pawn pawn = ritual.assignments.FirstAssignedPawn("mother");
		float value = (pawn?.health?.hediffSet?.GetFirstHediff<Hediff_Labor>())?.Progress ?? (pawn?.health?.hediffSet?.GetFirstHediff<Hediff_LaborPushing>())?.Progress ?? 1f;
		ritual.progressBarOverride = value;
		base.Tick(ritual);
	}

	public override string ExpectedDuration(Precept_Ritual ritual, RitualRoleAssignments assignments, float quality)
	{
		return "UntilBabyIsBorn".Translate();
	}

	protected override LordJob CreateLordJob(TargetInfo target, Pawn organizer, Precept_Ritual ritual, RitualObligation obligation, RitualRoleAssignments assignments)
	{
		IntVec3 bedSleepingSlotPosFor = RestUtility.GetBedSleepingSlotPosFor(assignments.FirstAssignedPawn("mother"), (Building_Bed)target.Thing);
		return new LordJob_Ritual_ChildBirth(target, ritual, obligation, def.stages, assignments, null, bedSleepingSlotPosFor);
	}

	public override bool ShouldInitAsSpectator(Pawn pawn, RitualRoleAssignments assignments)
	{
		Pawn pawn2 = assignments.FirstAssignedPawn("mother");
		if (pawn2 == null)
		{
			return false;
		}
		foreach (DirectPawnRelation item in LovePartnerRelationUtility.ExistingLovePartners(pawn2, allowDead: false))
		{
			if (item.otherPawn == pawn)
			{
				return true;
			}
		}
		return false;
	}

	public override bool TargetStillAllowed(TargetInfo selectedTarget, LordJob_Ritual ritual)
	{
		if (!base.TargetStillAllowed(selectedTarget, ritual))
		{
			return false;
		}
		Pawn pawn = ritual.assignments.FirstAssignedPawn("mother");
		Pawn pawn2 = ritual.assignments.FirstAssignedPawn("doctor");
		Building_Bed building_Bed = (Building_Bed)selectedTarget.Thing;
		ReservationManager reservationManager = building_Bed.Map.reservationManager;
		bool flag = reservationManager.ReservedBy(building_Bed, pawn);
		bool flag2 = reservationManager.ReservedBy(building_Bed, pawn2);
		if (ritual.StageIndex >= 2 && !flag2 && !flag)
		{
			return false;
		}
		if (flag && !PregnancyUtility.IsUsableBedFor(pawn, pawn, building_Bed))
		{
			return false;
		}
		if (flag2 && !PregnancyUtility.IsUsableBedFor(pawn, pawn2, building_Bed))
		{
			return false;
		}
		if (!RestUtility.CanUseBedNow(building_Bed, pawn, checkSocialProperness: false))
		{
			return false;
		}
		return true;
	}
}
