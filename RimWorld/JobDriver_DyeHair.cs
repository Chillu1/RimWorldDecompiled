using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_DyeHair : JobDriver
	{
		public const TargetIndex StylingStationInd = TargetIndex.A;

		public const TargetIndex DyeInd = TargetIndex.B;

		public const int WorkTimeTicks = 300;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			LocalTargetInfo target = job.GetTarget(TargetIndex.A);
			if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			Thing thing = target.Thing;
			if (thing != null && thing.def.hasInteractionCell && !pawn.ReserveSittableOrSpot(thing.InteractionCell, job, errorOnFailed))
			{
				return false;
			}
			Thing thing2 = job.GetTarget(TargetIndex.B).Thing;
			if (!pawn.Reserve(thing2, job, 1, 1, null, errorOnFailed))
			{
				return false;
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!ModLister.CheckIdeology("Hair dyeing"))
			{
				yield break;
			}
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			yield return Toils_General.Wait(300, TargetIndex.A).PlaySustainerOrSound(SoundDefOf.Interact_RecolorApparel).WithProgressBarToilDelay(TargetIndex.A);
			yield return Toils_General.Do(delegate
			{
				Thing thing = job.GetTarget(TargetIndex.B).Thing.SplitOff(1);
				if (thing != null && !thing.Destroyed)
				{
					thing.Destroy();
				}
				pawn.style.FinalizeHairColor();
			});
		}
	}
}
