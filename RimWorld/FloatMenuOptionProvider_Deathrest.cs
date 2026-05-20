using Verse;
using Verse.AI;

namespace RimWorld;

public class FloatMenuOptionProvider_Deathrest : FloatMenuOptionProvider
{
	protected override bool Drafted => true;

	protected override bool Undrafted => true;

	protected override bool Multiselect => false;

	protected override bool AppliesInt(FloatMenuContext context)
	{
		if (!ModsConfig.BiotechActive)
		{
			return false;
		}
		if (!context.FirstSelectedPawn.CanDeathrest())
		{
			return false;
		}
		return true;
	}

	protected override FloatMenuOption GetSingleOptionFor(Thing clickedThing, FloatMenuContext context)
	{
		Building_Bed bed = clickedThing as Building_Bed;
		if (bed == null || !bed.def.building.bed_humanlike)
		{
			return null;
		}
		if (!context.FirstSelectedPawn.CanReach(bed, PathEndMode.OnCell, Danger.Deadly))
		{
			return new FloatMenuOption("CannotDeathrest".Translate().CapitalizeFirst() + ": " + "NoPath".Translate().CapitalizeFirst(), null);
		}
		AcceptanceReport acceptanceReport = bed.CompAssignableToPawn.CanAssignTo(context.FirstSelectedPawn);
		if (!acceptanceReport.Accepted)
		{
			return new FloatMenuOption("CannotDeathrest".Translate().CapitalizeFirst() + ": " + acceptanceReport.Reason, null);
		}
		if ((!bed.CompAssignableToPawn.HasFreeSlot || !RestUtility.BedOwnerWillShare(bed, context.FirstSelectedPawn, context.FirstSelectedPawn.guest.GuestStatus)) && !bed.IsOwner(context.FirstSelectedPawn))
		{
			return new FloatMenuOption("CannotDeathrest".Translate().CapitalizeFirst() + ": " + "AssignedToOtherPawn".Translate(bed).CapitalizeFirst(), null);
		}
		bool flag = false;
		foreach (IntVec3 item in bed.OccupiedRect())
		{
			if (item.GetRoof(bed.Map) == null)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			return new FloatMenuOption("CannotDeathrest".Translate().CapitalizeFirst() + ": " + "ThingIsSkyExposed".Translate(bed).CapitalizeFirst(), null);
		}
		if (!RestUtility.IsValidBedFor(bed, context.FirstSelectedPawn, context.FirstSelectedPawn, checkSocialProperness: true, allowMedBedEvenIfSetToNoCare: false, ignoreOtherReservations: false, context.FirstSelectedPawn.GuestStatus))
		{
			return null;
		}
		return new FloatMenuOption("StartDeathrest".Translate(), delegate
		{
			Job job = JobMaker.MakeJob(JobDefOf.Deathrest, bed);
			job.forceSleep = true;
			context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
		});
	}
}
