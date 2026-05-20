using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_StudyInteract : JobDriver
{
	private const TargetIndex ThingToStudyIndex = TargetIndex.A;

	private const TargetIndex AdjacentCellIndex = TargetIndex.B;

	private const int BaseDurationTicks = 600;

	private const int PawnTargetStudyDurationFactor = 2;

	public const float ElectroharvesterFactor = 0.5f;

	private int studyInteractions;

	private Building_HoldingPlatform Platform => base.TargetThingA as Building_HoldingPlatform;

	private Thing ThingToStudy => Platform?.HeldPawn ?? base.TargetThingA;

	private CompStudiable StudyComp => ThingToStudy?.TryGetComp<CompStudiable>();

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (Platform != null)
		{
			if (pawn.Reserve(Platform, job, 1, -1, null, errorOnFailed))
			{
				return pawn.Reserve(ThingToStudy, job, 1, -1, null, errorOnFailed);
			}
			return false;
		}
		return pawn.Reserve(base.TargetThingA, job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		bool targetIsPawn = base.TargetThingA is Pawn;
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOn(() => (StudyComp == null || !StudyComp.CurrentlyStudiable()) ? true : false);
		AddFinishAction(delegate
		{
			if (StudyComp != null)
			{
				StudyComp.studyInteractions += studyInteractions;
			}
		});
		StatDef stat = ((Platform != null) ? StatDefOf.EntityStudyRate : StatDefOf.ResearchSpeed);
		int num = Mathf.RoundToInt(600f / pawn.GetStatValue(stat));
		if (targetIsPawn || Platform != null)
		{
			num *= 2;
		}
		Toil findAdjacentCell = Toils_General.Do(delegate
		{
			IntVec3 adjacentInteractionCell = SocialInteractionUtility.GetAdjacentInteractionCell(pawn, job.GetTarget(TargetIndex.A).Thing, job.playerForced);
			pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, adjacentInteractionCell);
			job.targetB = adjacentInteractionCell;
		});
		Toil goToAdjacentCell = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
		Toil studyToil;
		if (targetIsPawn)
		{
			studyToil = StudyPawn(TargetIndex.A, num);
		}
		else
		{
			studyToil = Toils_General.WaitWith(TargetIndex.A, num, useProgressBar: true, maintainPosture: false, maintainSleep: false, TargetIndex.A);
			studyToil.AddPreTickIntervalAction(delegate(int delta)
			{
				ThingToStudy.TryGetComp<CompObelisk>()?.Notify_InteractedTick(pawn, delta);
			});
		}
		studyToil.AddPreTickIntervalAction(delegate(int delta)
		{
			pawn.skills.Learn(SkillDefOf.Intellectual, 0.1f * (float)delta);
		});
		studyToil.activeSkill = () => SkillDefOf.Intellectual;
		if (StudyComp.KnowledgeCategory != null)
		{
			studyToil.WithEffect(() => EffecterDefOf.StudyHoraxian, base.TargetThingA);
		}
		Toil finishInteraction = ToilMaker.MakeToil("Interaction finish");
		finishInteraction.initAction = DoStudyInteraction;
		yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
		int interactions = 5 - StudyComp.studyInteractions;
		for (int i = 0; i < interactions; i++)
		{
			if (!targetIsPawn && i > 0)
			{
				yield return findAdjacentCell;
				yield return goToAdjacentCell;
			}
			yield return studyToil;
			yield return finishInteraction;
		}
		Toil toil = ToilMaker.MakeToil("Study finish");
		toil.initAction = delegate
		{
			studyInteractions = 0;
			StudyComp.studyInteractions = 0;
			StudyComp.lastStudiedTick = Find.TickManager.TicksGame;
			if (ModsConfig.AnomalyActive && ThingToStudy is Pawn pawn && (!pawn.RaceProps.Humanlike || pawn.IsMutant))
			{
				TaleRecorder.RecordTale(TaleDefOf.StudiedEntity, base.pawn, pawn);
			}
		};
		yield return toil;
	}

	private Toil StudyPawn(TargetIndex pawnIndex, int duration)
	{
		Toil study = ToilMaker.MakeToil("Study pawn");
		study.initAction = delegate
		{
			Pawn actor = study.actor;
			actor.pather.StopDead();
			if (actor.CurJob.GetTarget(pawnIndex).Thing is Pawn recipient)
			{
				PawnUtility.ForceWait(recipient, duration, study.actor);
				actor.interactions.TryInteractWith(recipient, InteractionDefOf.PrisonerStudyAnomaly);
			}
		};
		study.tickIntervalAction = delegate
		{
			study.actor.rotationTracker.FaceTarget(study.actor.CurJob.GetTarget(pawnIndex));
		};
		study.handlingFacing = true;
		study.defaultCompleteMode = ToilCompleteMode.Delay;
		study.defaultDuration = duration;
		study.WithProgressBarToilDelay(pawnIndex);
		return study;
	}

	private void DoStudyInteraction()
	{
		float anomalyKnowledgeAmount = 0f;
		if (ModsConfig.AnomalyActive)
		{
			anomalyKnowledgeAmount = StudyComp.AdjustedAnomalyKnowledgePerStudy * base.pawn.GetStatValue(StatDefOf.StudyEfficiency);
		}
		StudyComp.Study(base.pawn, 0.87f, anomalyKnowledgeAmount);
		studyInteractions++;
		if (ModsConfig.AnomalyActive && ThingToStudy is Pawn pawn)
		{
			pawn.mindState.lastAssignedInteractTime = Find.TickManager.TicksGame;
			pawn.mindState.interactionsToday++;
		}
	}

	public override bool? IsSameJobAs(Job j)
	{
		return j.targetA == base.TargetThingA;
	}

	protected override string ReportStringProcessed(string str)
	{
		return JobUtility.GetResolvedJobReport(str, ThingToStudy);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref studyInteractions, "studyInteractions", 0);
	}
}
