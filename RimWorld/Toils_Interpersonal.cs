using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class Toils_Interpersonal
{
	public static Toil GotoInteractablePosition(TargetIndex target)
	{
		Toil toil = ToilMaker.MakeToil("GotoInteractablePosition");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Pawn pawn = (Pawn)(Thing)actor.CurJob.GetTarget(target);
			if (SocialInteractionUtility.IsGoodPositionForInteraction(actor, pawn))
			{
				actor.jobs.curDriver.ReadyForNextToil();
			}
			else
			{
				actor.pather.StartPath(pawn, PathEndMode.Touch);
			}
		};
		toil.tickIntervalAction = delegate
		{
			Pawn actor = toil.actor;
			Pawn pawn = (Pawn)(Thing)actor.CurJob.GetTarget(target);
			Map map = actor.Map;
			if (SocialInteractionUtility.IsGoodPositionForInteraction(actor, pawn) && actor.Position.InHorDistOf(pawn.Position, Mathf.CeilToInt(3f)) && (!actor.pather.Moving || actor.pather.nextCell.GetDoor(map) == null))
			{
				actor.pather.StopDead();
				actor.jobs.curDriver.ReadyForNextToil();
			}
			else if (!actor.pather.Moving)
			{
				IntVec3 intVec = SocialInteractionUtility.BestInteractableCell(actor, pawn);
				if (intVec.IsValid)
				{
					actor.pather.StartPath(intVec, PathEndMode.OnCell);
				}
				else
				{
					actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
				}
			}
		};
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		return toil;
	}

	public static Toil GotoPrisoner(Pawn pawn, Pawn talkee, PrisonerInteractionModeDef mode)
	{
		Toil toil = ToilMaker.MakeToil("GotoPrisoner");
		toil.initAction = delegate
		{
			pawn.pather.StartPath(talkee, PathEndMode.Touch);
		};
		toil.AddFailCondition(delegate
		{
			if (talkee.DestroyedOrNull())
			{
				return true;
			}
			if (mode.mustBeAwake && !talkee.Awake())
			{
				return true;
			}
			if (!talkee.IsPrisonerOfColony)
			{
				return true;
			}
			return (talkee.guest == null || talkee.guest.IsInteractionDisabled(mode)) ? true : false;
		});
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		return toil;
	}

	public static Toil GotoGuiltyColonist(Pawn pawn, Pawn talkee)
	{
		Toil toil = ToilMaker.MakeToil("GotoGuiltyColonist");
		toil.initAction = delegate
		{
			pawn.pather.StartPath(talkee, PathEndMode.Touch);
		};
		toil.AddFailCondition(delegate
		{
			if (talkee.DestroyedOrNull())
			{
				return true;
			}
			return !talkee.guilt.IsGuilty;
		});
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		return toil;
	}

	public static Toil GotoSlave(Pawn pawn, Pawn talkee)
	{
		Toil toil = ToilMaker.MakeToil("GotoSlave");
		toil.initAction = delegate
		{
			pawn.pather.StartPath(talkee, PathEndMode.Touch);
		};
		toil.AddFailCondition(delegate
		{
			if (talkee.DestroyedOrNull())
			{
				return true;
			}
			return !talkee.IsSlaveOfColony;
		});
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
		return toil;
	}

	public static Toil WaitToBeAbleToInteract(Pawn pawn)
	{
		Toil toil = ToilMaker.MakeToil("WaitToBeAbleToInteract");
		toil.initAction = delegate
		{
			if (!pawn.interactions.InteractedTooRecentlyToInteract())
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
		};
		toil.tickIntervalAction = delegate
		{
			if (!pawn.interactions.InteractedTooRecentlyToInteract())
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
		};
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Never;
		return toil;
	}

	public static Toil ConvinceRecruitee(Pawn pawn, Pawn talkee, InteractionDef interactionDef = null)
	{
		Toil toil = ToilMaker.MakeToil("ConvinceRecruitee");
		toil.initAction = delegate
		{
			if (!pawn.interactions.TryInteractWith(talkee, interactionDef ?? InteractionDefOf.BuildRapport))
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
			else
			{
				pawn.records.Increment(RecordDefOf.PrisonersChatted);
			}
		};
		toil.FailOn(() => !talkee.guest.ScheduledForInteraction);
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 350;
		toil.activeSkill = () => SkillDefOf.Social;
		return toil;
	}

	public static Toil Interrogate(Pawn pawn, Pawn talkee)
	{
		Toil toil = ToilMaker.MakeToil("Interrogate");
		toil.initAction = delegate
		{
			if (!pawn.interactions.TryInteractWith(talkee, InteractionDefOf.InterrogateIdentity))
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
			else
			{
				pawn.records.Increment(RecordDefOf.PrisonersChatted);
			}
		};
		toil.FailOn(() => !talkee.guest.ScheduledForInteraction);
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 350;
		toil.activeSkill = () => SkillDefOf.Social;
		return toil;
	}

	public static Toil ReduceWill(Pawn pawn, Pawn talkee)
	{
		Toil toil = ToilMaker.MakeToil("ReduceWill");
		toil.initAction = delegate
		{
			if (!pawn.interactions.TryInteractWith(talkee, InteractionDefOf.ReduceWill))
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
			else
			{
				pawn.records.Increment(RecordDefOf.PrisonersChatted);
			}
		};
		toil.FailOn(() => !talkee.guest.ScheduledForInteraction);
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 350;
		toil.activeSkill = () => SkillDefOf.Social;
		return toil;
	}

	public static Toil SetLastInteractTime(TargetIndex targetInd)
	{
		Toil toil = ToilMaker.MakeToil("SetLastInteractTime");
		toil.initAction = delegate
		{
			Pawn obj = (Pawn)toil.actor.jobs.curJob.GetTarget(targetInd).Thing;
			obj.mindState.lastAssignedInteractTime = Find.TickManager.TicksGame;
			obj.mindState.interactionsToday++;
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		return toil;
	}

	public static Toil TryRecruit(TargetIndex recruiteeInd)
	{
		Toil toil = ToilMaker.MakeToil("TryRecruit");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(recruiteeInd).Thing;
			if (pawn.Spawned && pawn.Awake())
			{
				InteractionDef intDef = (pawn.AnimalOrWildMan() ? InteractionDefOf.TameAttempt : InteractionDefOf.RecruitAttempt);
				actor.interactions.TryInteractWith(pawn, intDef);
			}
		};
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 350;
		toil.activeSkill = () => (!((Pawn)toil.actor.jobs.curJob.GetTarget(recruiteeInd).Thing).RaceProps.Animal) ? SkillDefOf.Social : SkillDefOf.Animals;
		return toil;
	}

	public static Toil TryTrain(TargetIndex traineeInd)
	{
		Toil toil = ToilMaker.MakeToil("TryTrain");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(traineeInd).Thing;
			if (pawn.Spawned && pawn.Awake() && actor.interactions.TryInteractWith(pawn, InteractionDefOf.TrainAttempt))
			{
				float statValue = actor.GetStatValue(StatDefOf.TrainAnimalChance);
				statValue *= GenMath.LerpDouble(0f, 1f, 1.5f, 0.5f, pawn.GetStatValue(StatDefOf.Wildness));
				if (actor.relations.DirectRelationExists(PawnRelationDefOf.Bond, pawn))
				{
					statValue *= 5f;
				}
				statValue = Mathf.Clamp01(statValue);
				TrainableDef trainableDef = pawn.training.NextTrainableToTrain();
				if (trainableDef == null)
				{
					Log.ErrorOnce("Attempted to train untrainable animal", 7842936);
				}
				else
				{
					string text;
					if (Rand.Value < statValue)
					{
						pawn.training.Train(trainableDef, actor);
						if (pawn.caller != null)
						{
							pawn.caller.DoCall();
						}
						text = "TextMote_TrainSuccess".Translate(trainableDef.LabelCap, statValue.ToStringPercent());
						RelationsUtility.TryDevelopBondRelation(actor, pawn, 0.007f);
						TaleRecorder.RecordTale(TaleDefOf.TrainedAnimal, actor, pawn, trainableDef);
					}
					else
					{
						text = "TextMote_TrainFail".Translate(trainableDef.LabelCap, statValue.ToStringPercent());
					}
					text = text + "\n" + pawn.training.GetSteps(trainableDef) + " / " + trainableDef.steps;
					MoteMaker.ThrowText((actor.DrawPos + pawn.DrawPos) / 2f, actor.Map, text, 5f);
				}
			}
		};
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 100;
		toil.activeSkill = () => SkillDefOf.Animals;
		return toil;
	}

	public static Toil Interact(TargetIndex otherPawnInd, InteractionDef interaction)
	{
		Toil toil = ToilMaker.MakeToil("Interact");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(otherPawnInd).Thing;
			if (pawn.Spawned)
			{
				actor.interactions.TryInteractWith(pawn, interaction);
			}
		};
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 60;
		return toil;
	}

	public static Toil TryEnslave(TargetIndex prisonerInd)
	{
		Toil toil = ToilMaker.MakeToil("TryEnslave");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(prisonerInd).Thing;
			if (pawn.Spawned && pawn.Awake())
			{
				actor.interactions.TryInteractWith(pawn, InteractionDefOf.EnslaveAttempt);
			}
		};
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 350;
		toil.activeSkill = () => SkillDefOf.Social;
		return toil;
	}

	public static Toil TryConvert(TargetIndex prisonerInd)
	{
		Toil toil = ToilMaker.MakeToil("TryConvert");
		toil.initAction = delegate
		{
			Pawn actor = toil.actor;
			Pawn recipient = (Pawn)actor.jobs.curJob.GetTarget(prisonerInd).Thing;
			actor.interactions.TryInteractWith(recipient, InteractionDefOf.ConvertIdeoAttempt);
		};
		toil.socialMode = RandomSocialMode.Off;
		toil.defaultCompleteMode = ToilCompleteMode.Delay;
		toil.defaultDuration = 350;
		toil.activeSkill = () => SkillDefOf.Social;
		return toil;
	}

	public static Toil ExtractHemogen(TargetIndex prisonerInd, float bloodLoss)
	{
		Toil toil = ToilMaker.MakeToil("ExtractHemogen");
		toil.initAction = delegate
		{
			GenPlace.TryPlaceThing(ThingMaker.MakeThing(ThingDefOf.HemogenPack), toil.actor.Position, toil.actor.Map, ThingPlaceMode.Near);
			Pawn pawn = (Pawn)toil.actor.jobs.curJob.GetTarget(prisonerInd).Thing;
			Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
			hediff.Severity = bloodLoss;
			pawn.health.AddHediff(hediff);
		};
		return toil;
	}
}
