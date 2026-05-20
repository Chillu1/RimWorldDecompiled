using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_GoSwimming : JobDriver
{
	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	private bool NextDestIsOutdoorsAndNotEnjoyable()
	{
		if (base.Map == null)
		{
			return false;
		}
		if (!job.targetA.IsValid)
		{
			return false;
		}
		Room room = job.targetA.Cell.GetRoom(base.Map);
		if (room == null || !room.PsychologicallyOutdoors)
		{
			return false;
		}
		return !JoyGiver_GoSwimming.HappyToSwimOutsideOnMap(base.Map);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOn(NextDestIsOutdoorsAndNotEnjoyable);
		Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		toil.tickIntervalAction = delegate
		{
			if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
			{
				EndJobWith(JobCondition.Succeeded);
			}
			else
			{
				CheckForSwimmingPose();
				pawn.mindState.lastSwamTick = GenTicks.TicksGame;
			}
		};
		toil.AddPreInitAction(delegate
		{
			job.locomotionUrgency = LocomotionUrgency.Jog;
		});
		toil.AddFinishAction(delegate
		{
			job.locomotionUrgency = LocomotionUrgency.Walk;
		});
		yield return toil;
		Toil goToil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
		goToil.tickIntervalAction = delegate(int delta)
		{
			if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
			{
				EndJobWith(JobCondition.Succeeded);
			}
			else
			{
				CheckForSwimmingPose();
				pawn.mindState.lastSwamTick = GenTicks.TicksGame;
				JoyUtility.JoyTickCheckEnd(pawn, delta);
			}
		};
		goToil.AddFinishAction(CheckForSwimmingPose);
		yield return goToil;
		Toil toil2 = Toils_General.Wait(240);
		toil2.tickIntervalAction = delegate(int delta)
		{
			CheckForSwimmingPose();
			JoyUtility.JoyTickCheckEnd(pawn, delta);
			pawn.mindState.lastSwamTick = GenTicks.TicksGame;
		};
		yield return toil2;
		Toil toil3 = ToilMaker.MakeToil("MakeNewToils");
		toil3.initAction = delegate
		{
			if (job.targetQueueA.Count > 0)
			{
				if (pawn.health?.hediffSet != null && pawn.health.hediffSet.TryGetHediff(HediffDefOf.Heatstroke, out var hediff))
				{
					pawn.health.RemoveHediff(hediff);
				}
				LocalTargetInfo targetA = job.targetQueueA[0];
				job.targetQueueA.RemoveAt(0);
				job.targetA = targetA;
				JumpToToil(goToil);
			}
		};
		yield return toil3;
	}

	private void CheckForSwimmingPose()
	{
		if (pawn.Position.GetTerrain(pawn.Map).IsWater)
		{
			job.swimming = true;
		}
		else
		{
			job.swimming = false;
		}
	}
}
