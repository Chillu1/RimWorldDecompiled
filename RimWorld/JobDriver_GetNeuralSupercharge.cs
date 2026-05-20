using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_GetNeuralSupercharge : JobDriver
	{
		private const TargetIndex ChargerInd = TargetIndex.A;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A).Thing, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_General.Wait(0.55f.SecondsToTicks());
			Toil toil = Toils_General.Wait(1.5f.SecondsToTicks());
			toil.WithEffect(() => EffecterDefOf.NeuralSuperchargerUse, job.GetTarget(TargetIndex.A).Thing);
			yield return toil;
			yield return Toils_General.Do(delegate
			{
				(job.GetTarget(TargetIndex.A).Thing?.TryGetComp<CompRechargeable>())?.Discharge();
				pawn.health.AddHediff(HediffDefOf.NeuralSupercharge, pawn.health.hediffSet.GetBrain());
			});
			yield return Toils_General.Wait(0.35f.SecondsToTicks());
		}
	}
}
