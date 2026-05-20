using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Flick : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOn(() => base.Map.designationManager.DesignationOn(base.TargetThingA, DesignationDefOf.Flick) == null);
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		yield return Toils_General.Wait(15).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		Toil finalize = ToilMaker.MakeToil("MakeNewToils");
		finalize.initAction = delegate
		{
			Pawn actor = finalize.actor;
			ThingWithComps thingWithComps = (ThingWithComps)actor.CurJob.targetA.Thing;
			for (int i = 0; i < thingWithComps.AllComps.Count; i++)
			{
				if (thingWithComps.AllComps[i] is CompFlickable compFlickable && compFlickable.WantsFlick())
				{
					compFlickable.DoFlick();
				}
			}
			actor.records.Increment(RecordDefOf.SwitchesFlicked);
			base.Map.designationManager.DesignationOn(thingWithComps, DesignationDefOf.Flick)?.Delete();
		};
		finalize.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return finalize;
	}
}
