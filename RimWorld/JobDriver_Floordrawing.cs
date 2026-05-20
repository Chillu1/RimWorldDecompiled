using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Floordrawing : JobDriver
{
	private const int DrawingIntervalTicks = 2500;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (pawn.Reserve(base.TargetA, job, 1, -1, null, errorOnFailed))
		{
			return pawn.Reserve(base.TargetB, job, 1, -1, null, errorOnFailed);
		}
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnForbidden(TargetIndex.A);
		this.FailOnForbidden(TargetIndex.B);
		this.FailOnChildLearningConditions();
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			pawn.jobs.posture = PawnPosture.Standing;
		};
		toil.handlingFacing = true;
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.rotationTracker.FaceCell(base.TargetB.Cell);
			if (!LearningUtility.LearningTickCheckEnd(pawn, delta) && pawn.IsHashIntervalTick(2500, delta))
			{
				if (!FilthMaker.TryMakeFilth(base.TargetB.Cell, pawn.Map, ThingDefOf.Filth_Floordrawing))
				{
					pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
				}
				else
				{
					List<Thing> thingList = base.TargetB.Cell.GetThingList(pawn.Map);
					for (int i = 0; i < thingList.Count; i++)
					{
						if (thingList[i].def == ThingDefOf.Filth_Floordrawing)
						{
							pawn.Reserve(thingList[i], job);
						}
					}
				}
			}
		};
		toil.WithEffect(EffecterDefOf.Floordrawing, TargetIndex.A);
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = job.def.learningDuration;
		yield return toil;
	}
}
