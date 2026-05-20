using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobDriver_Workwatching : JobDriver
{
	private const TargetIndex AdultInd = TargetIndex.A;

	private const int FollowDistance = 3;

	private const int StopWatchingNoSkillJobTicks = 5;

	private const float LearningObservationDistance = 12f;

	private int consecutiveTicksNoSkillJob;

	private Pawn AdultToFollow => (Pawn)base.TargetThingA;

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return true;
	}

	protected override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnDespawnedOrNull(TargetIndex.A);
		this.FailOnChildLearningConditions();
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.tickIntervalAction = delegate(int delta)
		{
			Pawn adultToFollow = AdultToFollow;
			if (!LearningGiver_Workwatching.ChildCanLearnFromAdultJob(base.pawn, adultToFollow))
			{
				consecutiveTicksNoSkillJob += delta;
				if (consecutiveTicksNoSkillJob >= 5)
				{
					Pawn pawn = LearningGiver_Workwatching.AdultToWorkwatch(base.pawn);
					if (pawn == null)
					{
						EndJobWith(JobCondition.Incompletable);
					}
					else
					{
						consecutiveTicksNoSkillJob = 0;
						job.SetTarget(TargetIndex.A, pawn);
					}
					return;
				}
			}
			else
			{
				consecutiveTicksNoSkillJob = 0;
				if (base.pawn.Spawned && base.pawn.Map == adultToFollow.MapHeld && base.pawn.Position.InHorDistOf(adultToFollow.Position, 12f))
				{
					float num = LearningUtility.LearningRateFactor(base.pawn) * LearningDesireDefOf.Workwatching.xpPerTick * (float)delta;
					SkillDef skillDef = adultToFollow.CurJob?.RecipeDef?.workSkill;
					if (skillDef != null)
					{
						base.pawn.skills.Learn(skillDef, num);
					}
					else
					{
						List<SkillDef> list = adultToFollow.CurJob?.workGiverDef?.workType?.relevantSkills;
						num /= (float)list.Count;
						foreach (SkillDef item in list)
						{
							base.pawn.skills.Learn(item, num);
						}
					}
				}
			}
			if (!LearningUtility.LearningTickCheckEnd(base.pawn, delta) && (!((adultToFollow.Position - base.pawn.Position).LengthHorizontal <= 3f) || !base.pawn.Position.WithinRegions(adultToFollow.Position, base.pawn.Map, 2, TraverseParms.For(base.pawn))))
			{
				if (!base.pawn.CanReach(adultToFollow, PathEndMode.Touch, Danger.Deadly) || adultToFollow.IsForbidden(base.pawn))
				{
					EndJobWith(JobCondition.Incompletable);
				}
				else if (!base.pawn.pather.Moving || base.pawn.pather.Destination != adultToFollow)
				{
					base.pawn.pather.StartPath(adultToFollow, PathEndMode.Touch);
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		yield return toil;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref consecutiveTicksNoSkillJob, "consecutiveTicksNoSkillJob", 0);
	}
}
