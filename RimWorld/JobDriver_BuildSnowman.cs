using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_BuildSnowman : JobDriver
{
	private float workLeft = -1000f;

	protected const int BaseWorkAmount = 2300;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
		Toil doWork = ToilMaker.MakeToil("MakeNewToils");
		doWork.initAction = delegate
		{
			workLeft = 2300f;
		};
		doWork.tickIntervalAction = delegate(int delta)
		{
			workLeft -= doWork.actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f * (float)delta;
			if (workLeft <= 0f)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Snowman);
				thing.SetFaction(pawn.Faction);
				GenSpawn.Spawn(thing, base.TargetLocA, base.Map);
				ReadyForNextToil();
			}
			else
			{
				JoyUtility.JoyTickCheckEnd(pawn, delta);
			}
		};
		doWork.defaultCompleteMode = ToilCompleteMode.Never;
		doWork.FailOn(() => !JoyUtility.EnjoyableOutsideNow(pawn));
		doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		doWork.activeSkill = () => SkillDefOf.Construction;
		yield return doWork;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref workLeft, "workLeft", 0f);
	}
}
