using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Lessontaking : JobDriver
{
	public const int TicksWaitForTeacher = 5000;

	public bool isReadyToLearn;

	public int waitingForTeacherTicks;

	private Thing Desk => base.TargetThingA;

	private Pawn Teacher => (Pawn)base.TargetThingB;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.ReserveSittableOrSpot(SchoolUtility.DeskSpotStudent(Desk), job, errorOnFailed);
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnChildLearningConditions();
		this.FailOn(delegate
		{
			if (Teacher == null)
			{
				if (++waitingForTeacherTicks > 5000)
				{
					return true;
				}
				return false;
			}
			waitingForTeacherTicks = 0;
			Job job = Teacher.jobs?.curJob;
			return (job == null || job.def != JobDefOf.Lessongiving || job.GetTarget(TargetIndex.B).Pawn != pawn || job.GetTarget(TargetIndex.A).Thing != Desk) ? true : false;
		});
		yield return Toils_Goto.GotoCell(SchoolUtility.DeskSpotStudent(Desk), PathEndMode.OnCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.GainComfortFromCellIfPossible(delta);
			isReadyToLearn = true;
			pawn.rotationTracker.FaceTarget(Desk);
			LearningUtility.LearningTickCheckEnd(pawn, delta);
		};
		toil.handlingFacing = true;
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.socialMode = RandomSocialMode.Off;
		yield return toil;
	}

	public override string GetReport()
	{
		string text = ReportStringProcessed(job.def.reportString);
		if (Teacher != null)
		{
			text += " " + "ReportTakingLessonFrom".Translate(Teacher.Named("TEACHER"));
			if (Teacher.jobs?.curDriver is JobDriver_Lessongiving { taughtSkill: not null } jobDriver_Lessongiving)
			{
				text = text + " (" + jobDriver_Lessongiving.taughtSkill.label + ")";
			}
		}
		return text;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref isReadyToLearn, "isReadyToLearn", defaultValue: false);
		Scribe_Values.Look(ref waitingForTeacherTicks, "waitForTeacherTicks", 0);
	}
}
