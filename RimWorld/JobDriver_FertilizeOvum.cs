using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_FertilizeOvum : JobDriver
{
	private const int FertilizeTicks = 180;

	public const TargetIndex OvumIndex = TargetIndex.A;

	private HumanOvum ovum => (HumanOvum)base.TargetThingA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed))
		{
			return false;
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (!ModsConfig.BiotechActive)
		{
			yield break;
		}
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedOrNull(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: false, failIfStackCountLessThanJobCount: true);
		Toil toil = Toils_General.Wait(180).WithProgressBarToilDelay(TargetIndex.A);
		toil.tickIntervalAction = delegate(int delta)
		{
			if (pawn.IsHashIntervalTick(100, delta))
			{
				FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Heart);
			}
		};
		yield return toil;
		yield return Toils_General.DoAtomic(delegate
		{
			Thing thing = ovum.ProduceEmbryo(pawn);
			if (thing != null)
			{
				GenPlace.TryPlaceThing(thing, pawn.PositionHeld, pawn.Map, ThingPlaceMode.Near);
				pawn.carryTracker.DestroyCarriedThing();
			}
			else if (PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				Messages.Message("MessageFerilizeFailed".Translate(pawn.Named("PAWN")) + ": " + "CombinedGenesExceedMetabolismLimits".Translate(), pawn, MessageTypeDefOf.NegativeEvent);
			}
		});
	}
}
