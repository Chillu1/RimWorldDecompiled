using System.Collections.Generic;
using RimWorld;

namespace Verse.AI
{
	public class JobDriver_ExtractToInventory : JobDriver
	{
		private const TargetIndex ContainerInd = TargetIndex.A;

		private const TargetIndex ContentsInd = TargetIndex.B;

		public const int OpenTicks = 300;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnDestroyedOrNull(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, job.GetTarget(TargetIndex.A).Thing.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_General.WaitWhileExtractingContents(TargetIndex.A, TargetIndex.B, 300);
			yield return Toils_General.Do(delegate
			{
				job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompThingContainer>().innerContainer.TryDropAll(pawn.Position, pawn.Map, ThingPlaceMode.Near);
			});
			yield return Toils_Haul.TakeToInventory(TargetIndex.B, job.count);
		}
	}
}
