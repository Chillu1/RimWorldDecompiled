using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Lessongiving : JobDriver
{
	private const int InteractionTicks = 900;

	private const int TopTeachableSkillCount = 4;

	public bool isReadyToTeach;

	public SkillDef taughtSkill;

	private Thing Desk => base.TargetThingA;

	private Pawn Student => (Pawn)base.TargetThingB;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		if (!ModsConfig.BiotechActive || !pawn.ReserveSittableOrSpot(SchoolUtility.DeskSpotTeacher(Desk), job, errorOnFailed))
		{
			return false;
		}
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
		this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
		this.FailOn(delegate
		{
			if (PawnUtility.WillSoonHaveBasicNeed(pawn, -0.05f))
			{
				return true;
			}
			Job job = Student?.jobs?.curJob;
			return (job == null || job.def != JobDefOf.Lessontaking || job.GetTarget(TargetIndex.B).Pawn != pawn || job.GetTarget(TargetIndex.A).Thing != Desk) ? true : false;
		});
		yield return Toils_Goto.GotoCell(SchoolUtility.DeskSpotTeacher(Desk), PathEndMode.OnCell);
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate(int delta)
		{
			pawn.GainComfortFromCellIfPossible(delta);
			isReadyToTeach = true;
			pawn.rotationTracker.FaceTarget(Desk);
			if (taughtSkill == null)
			{
				taughtSkill = ChooseSkill(Student);
			}
			if (taughtSkill == null)
			{
				EndJobWith(JobCondition.Incompletable);
			}
			else if (isReadyToTeach && Student?.jobs?.curDriver is JobDriver_Lessontaking { isReadyToLearn: not false })
			{
				if (pawn.IsHashIntervalTick(900, delta))
				{
					pawn.interactions.TryInteractWith(Student, taughtSkill.lessonInteraction);
					pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.GaveLesson, Student);
					Student.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.WasTaught, pawn);
				}
				float num = LearningDesireDefOf.Lessontaking.xpPerTick * LearningUtility.LearningRateFactor(Student);
				Student.skills.Learn(taughtSkill, num * (float)delta);
				pawn.skills.Learn(SkillDefOf.Social, 0.1f * (float)delta);
			}
		};
		toil.handlingFacing = true;
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		toil.socialMode = RandomSocialMode.Off;
		toil.activeSkill = () => SkillDefOf.Social;
		yield return toil;
	}

	private SkillDef ChooseSkill(Pawn student)
	{
		return (from s in pawn.skills.skills
			where !s.TotallyDisabled && s.def.lessonInteraction != null && !student.skills.GetSkill(s.def).TotallyDisabled
			orderby s descending
			select s).Take(4).RandomElement().def;
	}

	public override string GetReport()
	{
		string text = ((taughtSkill != null) ? (" (" + taughtSkill.label + ")") : "");
		return ReportStringProcessed(job.def.reportString) + text;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref isReadyToTeach, "isReadyToTeach", defaultValue: false);
		Scribe_Defs.Look(ref taughtSkill, "taughtSkill");
	}
}
