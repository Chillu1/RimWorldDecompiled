using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_UseItem : JobDriver
	{
		private int useDuration = -1;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref useDuration, "useDuration", 0);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			useDuration = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompUsable>().Props.useDuration;
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnIncapable(PawnCapacityDefOf.Manipulation);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil toil = Toils_General.Wait(useDuration);
			toil.WithProgressBarToilDelay(TargetIndex.A);
			toil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			if (job.targetB.IsValid)
			{
				toil.FailOnDespawnedOrNull(TargetIndex.B);
				CompTargetable compTargetable = job.GetTarget(TargetIndex.A).Thing.TryGetComp<CompTargetable>();
				if (compTargetable != null && compTargetable.Props.nonDownedPawnOnly)
				{
					toil.FailOnDownedOrDead(TargetIndex.B);
				}
			}
			yield return toil;
			Toil use = new Toil();
			use.initAction = delegate
			{
				Pawn actor = use.actor;
				actor.CurJob.targetA.Thing.TryGetComp<CompUsable>().UsedBy(actor);
			};
			use.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return use;
		}
	}
}
