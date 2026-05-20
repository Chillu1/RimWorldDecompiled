using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Reign : JobDriver_Meditate
{
	protected const TargetIndex FacingInd = TargetIndex.B;

	protected const int ApplyThoughtInitialTicks = 10000;

	private Building_Throne Throne => base.TargetThingA as Building_Throne;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Throne, job, 1, -1, null, errorOnFailed);
	}

	public override string GetReport()
	{
		return ReportStringProcessed(job.def.reportString) + ": " + Throne.LabelShort.CapitalizeFirst() + "." + PsyfocusPerDayReport();
	}

	public override bool CanBeginNowWhileLyingDown()
	{
		return false;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		yield return Toils_General.Do(delegate
		{
			job.SetTarget(TargetIndex.B, Throne.InteractionCell + Throne.Rotation.FacingCell);
		});
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
		toil.FailOn(() => RoomRoleWorker_ThroneRoom.Validate(Throne.GetRoom()) != null);
		toil.FailOn(() => !MeditationUtility.CanMeditateNow(pawn) || !MeditationUtility.SafeEnvironmentalConditions(pawn, base.TargetLocA, base.Map));
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = job.def.joyDuration;
		toil.tickAction = delegate
		{
			if (pawn.mindState.applyThroneThoughtsTick == 0)
			{
				pawn.mindState.applyThroneThoughtsTick = Find.TickManager.TicksGame + 10000;
			}
			else if (pawn.mindState.applyThroneThoughtsTick <= Find.TickManager.TicksGame)
			{
				pawn.mindState.applyThroneThoughtsTick = Find.TickManager.TicksGame + 60000;
				ThoughtDef thoughtDef = null;
				if (Throne.GetRoom().Role == RoomRoleDefOf.ThroneRoom)
				{
					thoughtDef = ThoughtDefOf.ReignedInThroneroom;
				}
				if (thoughtDef != null)
				{
					int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(Throne.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
					if (thoughtDef.stages[scoreStageIndex] != null)
					{
						pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
					}
				}
			}
			rotateToFace = TargetIndex.B;
			MeditationTick();
		};
		yield return toil;
	}
}
