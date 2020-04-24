using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Reign : JobDriver
	{
		private const TargetIndex ThroneInd = TargetIndex.A;

		private const TargetIndex FacingInd = TargetIndex.B;

		private const int JobEndInterval = 5000;

		private const int ApplyThoughtInitialTicks = 10000;

		private Building_Throne Throne => (Building_Throne)base.TargetThingA;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Throne, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return Toils_General.Do(delegate
			{
				job.SetTarget(TargetIndex.B, Throne.InteractionCell + Throne.Rotation.FacingCell);
			});
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil toil = new Toil();
			toil.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			toil.FailOn(() => pawn.needs.authority.CurLevel >= 1f);
			toil.FailOn(() => Throne.AssignedPawn != pawn);
			toil.FailOn(() => RoomRoleWorker_ThroneRoom.Validate(Throne.GetRoom()) != null);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 5000;
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
				pawn.GainComfortFromCellIfPossible();
			};
			yield return toil;
		}
	}
}
