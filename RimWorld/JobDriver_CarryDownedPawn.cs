using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_CarryDownedPawn : JobDriver
{
	private const TargetIndex TakeeIndex = TargetIndex.A;

	protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Takee, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOnAggroMentalStateAndHostile(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOn(() => !Takee.Downed && !Takee.IsSelfShutdown())
			.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		Toil toil = Toils_Haul.StartCarryThing(TargetIndex.A);
		toil.AddPreInitAction(CheckMakeTakeeGuest);
		yield return toil;
	}

	private void CheckMakeTakeeGuest()
	{
		if (!job.def.makeTargetPrisoner && !Takee.HostileTo(Faction.OfPlayer) && Takee.Faction != Faction.OfPlayer && Takee.HostFaction != Faction.OfPlayer && Takee.guest != null && !Takee.IsWildMan())
		{
			Takee.guest.SetGuestStatus(Faction.OfPlayer);
		}
	}
}
