using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld;

public class JobDriver_Meditate : JobDriver
{
	protected IntVec3 faceDir;

	private Mote psyfocusMote;

	protected Sustainer sustainer;

	protected const TargetIndex SpotInd = TargetIndex.A;

	protected const TargetIndex BedInd = TargetIndex.B;

	protected const TargetIndex FocusInd = TargetIndex.C;

	public static float AnimaTreeSubplantProgressPerTick = 6.666667E-05f;

	private const int TicksBetweenMotesBase = 100;

	private const int JobCheckIntervalTicks = 4000;

	public LocalTargetInfo Focus => job.GetTarget(TargetIndex.C);

	private bool FromBed => job.GetTarget(TargetIndex.B).IsValid;

	protected string PsyfocusPerDayReport()
	{
		if (!pawn.HasPsylink)
		{
			return "";
		}
		Thing thing = Focus.Thing;
		float f = MeditationUtility.PsyfocusGainPerTick(pawn, thing) * 60000f;
		return "\n" + "PsyfocusPerDayOfMeditation".Translate(f.ToStringPercent()).CapitalizeFirst();
	}

	public override string GetReport()
	{
		if (ModsConfig.RoyaltyActive)
		{
			Thing thing = Focus.Thing;
			if (thing != null && !thing.Destroyed)
			{
				return "MeditatingAt".Translate() + ": " + thing.LabelShort.CapitalizeFirst() + "." + PsyfocusPerDayReport();
			}
			return base.GetReport() + PsyfocusPerDayReport();
		}
		return base.GetReport();
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		Toil meditate = ToilMaker.MakeToil("MakeNewToils");
		meditate.socialMode = RandomSocialMode.Off;
		if (FromBed)
		{
			this.KeepLyingDown(TargetIndex.B);
			meditate = Toils_LayDown.LayDown(TargetIndex.B, job.GetTarget(TargetIndex.B).Thing is Building_Bed, lookForOtherJobs: false, canSleep: false);
		}
		else
		{
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			meditate.initAction = delegate
			{
				LocalTargetInfo target = job.GetTarget(TargetIndex.C);
				if (target.IsValid)
				{
					faceDir = target.Cell - pawn.Position;
				}
				else
				{
					faceDir = (job.def.faceDir.IsValid ? job.def.faceDir : Rot4.Random).FacingCell;
				}
			};
			if (Focus != null)
			{
				meditate.FailOnDespawnedOrNull(TargetIndex.C);
				meditate.FailOnForbidden(TargetIndex.A);
				if (pawn.HasPsylink && Focus.Thing != null)
				{
					meditate.FailOn(() => Focus.Thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, pawn) < float.Epsilon);
				}
			}
			meditate.handlingFacing = true;
		}
		meditate.defaultCompleteMode = ToilCompleteMode.Delay;
		meditate.defaultDuration = job.def.joyDuration;
		meditate.FailOn(() => !MeditationUtility.CanMeditateNow(pawn) || !MeditationUtility.SafeEnvironmentalConditions(pawn, base.TargetLocA, base.Map));
		meditate.AddPreTickAction(delegate
		{
			bool flag = pawn.GetTimeAssignment() == TimeAssignmentDefOf.Meditate;
			if (job.ignoreJoyTimeAssignment)
			{
				Pawn_PsychicEntropyTracker psychicEntropy = pawn.psychicEntropy;
				bool flag2 = !flag && job.wasOnMeditationTimeAssignment;
				if (pawn.IsHashIntervalTick(4000) && psychicEntropy != null && !flag && psychicEntropy.CurrentPsyfocus >= Mathf.Max(psychicEntropy.TargetPsyfocus + 0.05f, 0.99f))
				{
					pawn.jobs.CheckForJobOverride();
					return;
				}
				if (flag2 && psychicEntropy.TargetPsyfocus < psychicEntropy.CurrentPsyfocus)
				{
					EndJobWith(JobCondition.InterruptForced);
					return;
				}
				job.psyfocusTargetLast = psychicEntropy.TargetPsyfocus;
				job.wasOnMeditationTimeAssignment = flag;
			}
			else if (pawn.needs.joy.CurLevelPercentage >= 1f)
			{
				EndJobWith(JobCondition.InterruptForced);
				return;
			}
			if (faceDir.IsValid && !FromBed)
			{
				pawn.rotationTracker.FaceCell(pawn.Position + faceDir);
			}
			MeditationTick();
			if (ModsConfig.RoyaltyActive && MeditationFocusDefOf.Natural.CanPawnUse(pawn))
			{
				int num = GenRadial.NumCellsInRadius(MeditationUtility.FocusObjectSearchRadius);
				for (int i = 0; i < num; i++)
				{
					IntVec3 c = pawn.Position + GenRadial.RadialPattern[i];
					if (c.InBounds(pawn.Map))
					{
						Plant plant = c.GetPlant(pawn.Map);
						if (plant != null && plant.def == ThingDefOf.Plant_TreeAnima)
						{
							plant.TryGetComp<CompSpawnSubplant>()?.AddProgress(AnimaTreeSubplantProgressPerTick);
						}
					}
				}
			}
		});
		yield return meditate;
	}

	public override void Notify_Starting()
	{
		base.Notify_Starting();
		if (ModsConfig.RoyaltyActive && pawn.psychicEntropy != null)
		{
			job.psyfocusTargetLast = pawn.psychicEntropy.TargetPsyfocus;
		}
	}

	protected void MeditationTick()
	{
		pawn.skills.Learn(SkillDefOf.Intellectual, 0.018000001f);
		pawn.GainComfortFromCellIfPossible(1);
		if (pawn.needs.joy != null)
		{
			JoyUtility.JoyTickCheckEnd(pawn, 1, JoyTickFullJoyAction.None);
		}
		if (pawn.IsHashIntervalTick(100))
		{
			FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Meditating);
		}
		if (!ModsConfig.RoyaltyActive || pawn.psychicEntropy == null)
		{
			return;
		}
		pawn.psychicEntropy.Notify_Meditated();
		if (pawn.HasPsylink && pawn.psychicEntropy.PsychicSensitivity > float.Epsilon)
		{
			float yOffset = (float)(pawn.Position.x % 2 + pawn.Position.z % 2) / 10f;
			if (psyfocusMote == null || psyfocusMote.Destroyed)
			{
				psyfocusMote = MoteMaker.MakeAttachedOverlay(pawn, ThingDefOf.Mote_PsyfocusPulse, Vector3.zero);
				psyfocusMote.yOffset = yOffset;
			}
			psyfocusMote.Maintain();
			if (sustainer == null || sustainer.Ended)
			{
				sustainer = SoundDefOf.MeditationGainPsyfocus.TrySpawnSustainer(SoundInfo.InMap(pawn, MaintenanceType.PerTick));
			}
			sustainer.Maintain();
			pawn.psychicEntropy.GainPsyfocus_NewTemp(1, Focus.Thing);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref faceDir, "faceDir");
	}
}
