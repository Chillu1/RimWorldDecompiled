using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobDriver_ActivateArchonexusCore : JobDriver_Goto
{
	private const int DefaultDuration = 120;

	protected override IEnumerable<Toil> MakeNewToils()
	{
		if (ModLister.CheckIdeology("Activate archonexus core"))
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell);
			Toil toil = ToilMaker.MakeToil("MakeNewToils");
			toil.initAction = delegate
			{
				((Building_ArchonexusCore)job.targetA.Thing).Activate();
			};
			toil.handlingFacing = true;
			toil.tickIntervalAction = delegate
			{
				pawn.rotationTracker.FaceTarget(base.TargetA);
			};
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 120;
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			yield return toil;
		}
	}
}
