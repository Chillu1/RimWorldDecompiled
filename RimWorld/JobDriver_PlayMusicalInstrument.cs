using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_PlayMusicalInstrument : JobDriver_SitFacingBuilding
	{
		public Building_MusicalInstrument MusicalInstrument => (Building_MusicalInstrument)(Thing)job.GetTarget(TargetIndex.A);

		protected override void ModifyPlayToil(Toil toil)
		{
			base.ModifyPlayToil(toil);
			toil.AddPreInitAction(delegate
			{
				MusicalInstrument.StartPlaying(pawn);
			});
			toil.AddFinishAction(delegate
			{
				MusicalInstrument.StopPlaying();
			});
		}
	}
}
