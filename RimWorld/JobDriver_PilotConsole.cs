using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_PilotConsole : JobDriver
{
	private const TargetIndex ConsoleInd = TargetIndex.A;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			Thing thing = job.GetTarget(TargetIndex.A).Thing;
			if (thing != null)
			{
				((Precept_Ritual)pawn.Ideo.GetPrecept(PreceptDefOf.GravshipLaunch)).ShowRitualBeginWindow(thing, null, pawn);
			}
		};
		yield return toil;
	}
}
