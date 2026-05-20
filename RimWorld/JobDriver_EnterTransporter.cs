using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

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
		this.FailOn(() => Shuttle != null && !Shuttle.IsAllowed(pawn));
		yield return Toils_Goto.GotoThing(TransporterInd, PathEndMode.Touch);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			if (job.playerForced || !LoadTransportersJobUtility.HasJobOnTransporter(pawn, Transporter))
			{
				if (!Transporter.LoadingInProgressOrReadyToLaunch)
				{
					TransporterUtility.InitiateLoading(Gen.YieldSingle(Transporter));
				}
				CompTransporter transporter = Transporter;
				bool flag = pawn.DeSpawnOrDeselect();
				transporter.GetDirectlyHeldThings().TryAdd(pawn);
				if (flag)
				{
					Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
				}
			}
		};
		yield return toil;
	}
}
