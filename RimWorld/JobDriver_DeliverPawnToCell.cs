using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobDriver_DeliverPawnToCell : JobDriver
{
	private const TargetIndex TakeeIndex = TargetIndex.A;

	private const TargetIndex TargetCellIndex = TargetIndex.B;

	protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		base.Map.reservationManager.ReleaseAllForTarget(Takee);
		return pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedOrNull(TargetIndex.A).FailOn(() => !pawn.CanReach(job.GetTarget(TargetIndex.B), PathEndMode.Touch, Danger.Deadly))
			.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		Toil startCarrying = Toils_Haul.StartCarryThing(TargetIndex.A);
		Toil gotoCell = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
		yield return Toils_Jump.JumpIf(gotoCell, () => pawn.IsCarryingPawn(Takee));
		yield return startCarrying;
		yield return gotoCell;
		yield return Toils_General.Do(delegate
		{
			if (!job.ritualTag.NullOrEmpty())
			{
				if (Takee.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual)
				{
					lordJob_Ritual.AddTagForPawn(Takee, job.ritualTag);
				}
				if (pawn.GetLord()?.LordJob is LordJob_Ritual lordJob_Ritual2)
				{
					lordJob_Ritual2.AddTagForPawn(pawn, job.ritualTag);
				}
			}
		});
		yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.B, null, storageMode: false);
	}
}
