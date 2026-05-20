using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_PlayMusicalInstrument : JobDriver_SitFacingBuilding
{
	public Building_MusicalInstrument MusicalInstrument => (Building_MusicalInstrument)(Thing)job.GetTarget(TargetIndex.A);

	protected override void ModifyPlayToil(Toil toil)
	{
		ModLister.CheckRoyaltyOrIdeology("Instrument");
		base.ModifyPlayToil(toil);
		toil.AddPreInitAction(delegate
		{
			MusicalInstrument?.StartPlaying(pawn);
			toil.tickIntervalAction = delegate(int delta)
			{
				toil.actor.rotationTracker.FaceTarget(toil.actor.CurJob.GetTarget((!base.TargetC.IsValid) ? TargetIndex.A : TargetIndex.C));
				pawn.GainComfortFromCellIfPossible(delta);
				JoyUtility.JoyTickCheckEnd(pawn, delta, JoyTickFullJoyAction.EndJob, 1f, MusicalInstrument);
			};
		});
		toil.handlingFacing = true;
		toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		toil.AddFinishAction(delegate
		{
			MusicalInstrument.StopPlaying();
		});
	}
}
