using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_TakeAndEnterPortal : JobDriver_EnterPortal
{
	private const TargetIndex ThingInd = TargetIndex.B;

	private Thing ThingToTake => job.GetTarget(TargetIndex.B).Thing;

	private Pawn PawnToTake => ThingToTake as Pawn;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(ThingToTake, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.B);
		this.FailOn(() => PawnToTake != null && !PawnToTake.Downed && PawnToTake.Awake());
		yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Construct.UninstallIfMinifiable(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
		yield return Toils_Haul.StartCarryThing(TargetIndex.B);
		foreach (Toil item in base.MakeNewToils())
		{
			yield return item;
		}
	}
}
