using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_CarryToEntityHolder : JobDriver
{
	private const TargetIndex DestHolderIndex = TargetIndex.A;

	private const TargetIndex TakeeIndex = TargetIndex.B;

	private const int EnterDelayTicks = 300;

	private Thing Takee => job.GetTarget(TargetIndex.B).Thing;

	private CompEntityHolder DestHolder => job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompEntityHolder>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(DestHolder.parent, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.B);
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(() => !DestHolder.Available);
		this.FailOn(() => Takee is Pawn pawn && !(pawn.GetComp<CompActivity>()?.IsDormant ?? true));
		this.FailOn(() => Takee.TryGetComp<CompHoldingPlatformTarget>().EntityHolder != DestHolder);
		if (pawn.carryTracker.CarriedThing != Takee)
		{
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
		}
		yield return Toils_Haul.StartCarryThing(TargetIndex.B);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
		foreach (Toil item in ChainTakeeToPlatformToils(pawn, Takee, DestHolder, TargetIndex.A))
		{
			yield return item;
		}
		yield return Toils_General.Do(delegate
		{
			if (Takee is Pawn pawn && (!pawn.RaceProps.Humanlike || pawn.IsMutant))
			{
				TaleRecorder.RecordTale(TaleDefOf.Captured, base.pawn, pawn);
			}
		});
	}

	public static IEnumerable<Toil> ChainTakeeToPlatformToils(Pawn taker, Thing takee, CompEntityHolder platform, TargetIndex platformIndex)
	{
		yield return Toils_Goto.GotoThing(platformIndex, PathEndMode.ClosestTouch);
		Toil toil = Toils_General.WaitWith(platformIndex, 300, useProgressBar: true);
		toil.PlaySustainerOrSound(SoundDefOf.ChainToPlatform);
		yield return toil;
		yield return Toils_General.Do(delegate
		{
			Thing thing = takee;
			platform.Container.TryAddOrTransfer(thing, 1);
			CompHoldingPlatformTarget compHoldingPlatformTarget = thing.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget != null)
			{
				compHoldingPlatformTarget.Notify_HeldOnPlatform(platform.Container);
				if (compHoldingPlatformTarget.Props.capturedLetterLabel != null)
				{
					Find.LetterStack.ReceiveLetter(compHoldingPlatformTarget.Props.capturedLetterLabel, compHoldingPlatformTarget.Props.capturedLetterText.Formatted(taker.Named("PAWN")), LetterDefOf.NeutralEvent, platform.parent);
				}
			}
		});
	}
}
