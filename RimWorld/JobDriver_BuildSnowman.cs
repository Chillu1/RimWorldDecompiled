using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
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
			JobDriver_BuildSnowman jobDriver_BuildSnowman = this;
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			Toil doWork = new Toil();
			doWork.initAction = delegate
			{
				jobDriver_BuildSnowman.workLeft = 2300f;
			};
			doWork.tickAction = delegate
			{
				jobDriver_BuildSnowman.workLeft -= doWork.actor.GetStatValue(StatDefOf.ConstructionSpeed) * 1.7f;
				if (jobDriver_BuildSnowman.workLeft <= 0f)
				{
					Thing thing = ThingMaker.MakeThing(ThingDefOf.Snowman);
					thing.SetFaction(jobDriver_BuildSnowman.pawn.Faction);
					GenSpawn.Spawn(thing, jobDriver_BuildSnowman.TargetLocA, jobDriver_BuildSnowman.Map);
					jobDriver_BuildSnowman.ReadyForNextToil();
				}
				else
				{
					JoyUtility.JoyTickCheckEnd(jobDriver_BuildSnowman.pawn);
				}
			};
			doWork.defaultCompleteMode = ToilCompleteMode.Never;
			doWork.FailOn(() => !JoyUtility.EnjoyableOutsideNow(jobDriver_BuildSnowman.pawn));
			doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			yield return doWork;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref workLeft, "workLeft", 0f);
		}
	}
}
