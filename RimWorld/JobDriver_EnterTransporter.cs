using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_EnterTransporter : JobDriver
	{
		private TargetIndex TransporterInd = TargetIndex.A;

		public CompTransporter Transporter => job.GetTarget(TransporterInd).Thing?.TryGetComp<CompTransporter>();

		public CompShuttle Shuttle => job.GetTarget(TransporterInd).Thing?.TryGetComp<CompShuttle>();

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TransporterInd);
			this.FailOn(() => !Transporter.LoadingInProgressOrReadyToLaunch);
			this.FailOn(() => Shuttle != null && !Shuttle.IsAllowedNow(pawn));
			yield return Toils_Goto.GotoThing(TransporterInd, PathEndMode.Touch);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				CompTransporter transporter = Transporter;
				pawn.DeSpawn();
				transporter.GetDirectlyHeldThings().TryAdd(pawn);
			};
			yield return toil;
		}
	}
}
