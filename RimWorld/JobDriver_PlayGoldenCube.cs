using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_PlayGoldenCube : JobDriver
{
	private IntVec3 nextGoal;

	private IntVec3 position;

	private int lastDist;

	private int nextMoveTick;

	private int stepsTillRotSwitch = SwitchDirStepsRange.RandomInRange;

	private bool clockwise;

	private float angle;

	private const int MinCircleDist = 1;

	private const int MaxCircleDist = 3;

	private static readonly IntRange SwitchDirStepsRange = new IntRange(6, 12);

	private static readonly IntRange WaitTimeTicksRange = new IntRange(180, 360);

	private const TargetIndex CubeIndex = TargetIndex.A;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.targetA, job, 1, 1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDestroyedOrNull(TargetIndex.A);
		this.FailOn(() => !pawn.health.hediffSet.HasHediff(HediffDefOf.CubeInterest));
		this.FailOn(() => !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation));
		yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
		Toil toil = Toils_Haul.StartCarryThing(TargetIndex.A);
		toil.AddPreInitAction(delegate
		{
			position = base.TargetThingA.PositionHeld;
		});
		yield return toil;
		Toil toil2 = ToilMaker.MakeToil("MakeNewToils");
		toil2.defaultCompleteMode = ToilCompleteMode.Delay;
		toil2.defaultDuration = job.def.joyDuration;
		Toil toil3 = toil2;
		toil3.initAction = (Action)Delegate.Combine(toil3.initAction, (Action)delegate
		{
			job.locomotionUrgency = LocomotionUrgency.Walk;
		});
		Toil toil4 = toil2;
		toil4.tickIntervalAction = (Action<int>)Delegate.Combine(toil4.tickIntervalAction, (Action<int>)delegate
		{
			Pawn actor = toil2.actor;
			if (GenTicks.TicksGame >= nextMoveTick && !actor.pather.Moving && TryGetNextCell(out nextGoal))
			{
				actor.pather.StartPath(nextGoal, PathEndMode.OnCell);
				nextMoveTick = GenTicks.TicksGame + WaitTimeTicksRange.RandomInRange;
			}
		});
		toil2.AddFinishAction(delegate
		{
			if (pawn.needs?.joy != null)
			{
				pawn.needs.joy.CurLevelPercentage = 1f;
				JoyUtility.TryGainRecRoomThought(pawn);
			}
			pawn.health.hediffSet.GetFirstHediff<Hediff_CubeInterest>().Notify_PlayedWith();
		});
		yield return toil2;
		yield return Toils_Haul.DropCarriedThing();
	}

	private bool TryGetNextCell(out IntVec3 dest)
	{
		dest = IntVec3.Invalid;
		IntVec3 intVec = position;
		int num = 0;
		int num2 = 0;
		if (stepsTillRotSwitch <= 0)
		{
			stepsTillRotSwitch = SwitchDirStepsRange.RandomInRange;
			clockwise = !clockwise;
		}
		do
		{
			angle = (angle + Rand.Range(45f, 90f) * (float)(clockwise ? 1 : (-1))) % 360f;
			for (int i = 1; i <= 3 && (intVec + IntVec3.FromPolar(angle, i)).Walkable(pawn.Map); i++)
			{
				num = i;
			}
		}
		while (num == 0 && num2++ < 10);
		if (num2 >= 10 || num == 0)
		{
			return false;
		}
		int num3 = Rand.Range(1, num + 1);
		dest = intVec + IntVec3.FromPolar(angle, num3);
		lastDist = num3;
		stepsTillRotSwitch--;
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref position, "position");
		Scribe_Values.Look(ref nextGoal, "nextGoal");
		Scribe_Values.Look(ref lastDist, "lastDist", 0);
		Scribe_Values.Look(ref stepsTillRotSwitch, "stepsTillSwitch", 0);
		Scribe_Values.Look(ref angle, "angle", 0f);
		Scribe_Values.Look(ref clockwise, "clockwise", defaultValue: false);
	}
}
