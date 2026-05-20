using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_ActivitySuppression : JobDriver
{
	private const TargetIndex ThingToSuppressOrPlatformIndex = TargetIndex.A;

	private const TargetIndex AdjacentCellIndex = TargetIndex.B;

	private Building_HoldingPlatform Platform => base.TargetThingA as Building_HoldingPlatform;

	private Thing ThingToSuppress => Platform?.HeldPawn ?? base.TargetThingA;

	protected override string ReportStringProcessed(string str)
	{
		return JobUtility.GetResolvedJobReport(str, LocalTargetInfo.Invalid, LocalTargetInfo.Invalid, ThingToSuppress);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (Platform != null)
		{
			if (pawn.Reserve(Platform, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(ThingToSuppress, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}
		return pawn.Reserve(ThingToSuppress, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		job.targetC = ThingToSuppress;
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(() => !ActivitySuppressionUtility.CanBeSuppressed(ThingToSuppress, considerMinActivity: false, job.playerForced));
		yield return Toils_General.Do(delegate
		{
			IntVec3 adjacentInteractionCell = SocialInteractionUtility.GetAdjacentInteractionCell(pawn, base.TargetThingA, job.playerForced);
			pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, adjacentInteractionCell);
			job.SetTarget(TargetIndex.B, adjacentInteractionCell);
		});
		yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
		yield return TrySuppress(ThingToSuppress);
	}

	private Toil TrySuppress(Thing thingToSuppress)
	{
		Toil toil = ToilMaker.MakeToil("TrySuppress");
		toil.initAction = delegate
		{
			toil.actor.pather.StopDead();
		};
		toil.tickIntervalAction = delegate(int delta)
		{
			Pawn actor = toil.actor;
			if (thingToSuppress == null)
			{
				Log.Error("Tried to execute activity suppression toil but thingToSuppress was null");
				actor.jobs.EndCurrentJob(JobCondition.Errored);
			}
			else
			{
				CompActivity compActivity = thingToSuppress.TryGetComp<CompActivity>();
				float num = actor.GetStatValue(StatDefOf.ActivitySuppressionRate) / 2500f * (float)delta;
				compActivity.AdjustActivity(0f - num);
				pawn.skills.Learn(SkillDefOf.Intellectual, 0.1f);
				if (compActivity.ActivityLevel < thingToSuppress.TryGetComp<CompActivity>().suppressIfAbove - 0.1f && actor.IsHashIntervalTick(4000, delta))
				{
					actor.jobs.CheckForJobOverride();
				}
				ThingToSuppress.TryGetComp<CompObelisk>()?.Notify_InteractedTick(pawn, delta);
			}
		};
		toil.WithProgressBar(TargetIndex.A, () => (thingToSuppress == null) ? 1f : (1f - thingToSuppress.TryGetComp<CompActivity>().ActivityLevel));
		toil.WithEffect(EffecterDefOf.ActivitySuppression, () => ThingToSuppress.SpawnedParentOrMe);
		toil.AddEndCondition(delegate
		{
			if (thingToSuppress == null)
			{
				Log.Error("Tried to execute activity suppression toil but thingToSuppress was null");
				return JobCondition.Errored;
			}
			return (!(thingToSuppress.TryGetComp<CompActivity>().ActivityLevel < 0.01f)) ? JobCondition.Ongoing : JobCondition.Succeeded;
		});
		toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		toil.activeSkill = () => SkillDefOf.Intellectual;
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		return toil;
	}
}
