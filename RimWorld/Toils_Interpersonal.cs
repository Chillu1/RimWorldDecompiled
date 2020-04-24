using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class Toils_Interpersonal
	{
		public static Toil GotoInteractablePosition(TargetIndex target)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor2 = toil.actor;
				Pawn pawn2 = (Pawn)(Thing)actor2.CurJob.GetTarget(target);
				if (InteractionUtility.IsGoodPositionForInteraction(actor2, pawn2))
				{
					actor2.jobs.curDriver.ReadyForNextToil();
				}
				else
				{
					actor2.pather.StartPath(pawn2, PathEndMode.Touch);
				}
			};
			toil.tickAction = delegate
			{
				Pawn actor = toil.actor;
				Pawn pawn = (Pawn)(Thing)actor.CurJob.GetTarget(target);
				Map map = actor.Map;
				if (InteractionUtility.IsGoodPositionForInteraction(actor, pawn) && actor.Position.InHorDistOf(pawn.Position, Mathf.CeilToInt(3f)) && (!actor.pather.Moving || actor.pather.nextCell.GetDoor(map) == null))
				{
					actor.pather.StopDead();
					actor.jobs.curDriver.ReadyForNextToil();
				}
				else if (!actor.pather.Moving)
				{
					IntVec3 intVec = IntVec3.Invalid;
					for (int i = 0; i < 9 && (i != 8 || !intVec.IsValid); i++)
					{
						IntVec3 intVec2 = pawn.Position + GenAdj.AdjacentCellsAndInside[i];
						if (intVec2.InBounds(map) && intVec2.Walkable(map) && intVec2 != actor.Position && InteractionUtility.IsGoodPositionForInteraction(intVec2, pawn.Position, map) && actor.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly) && (!intVec.IsValid || actor.Position.DistanceToSquared(intVec2) < actor.Position.DistanceToSquared(intVec)))
						{
							intVec = intVec2;
						}
					}
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
			Toil toil = new Toil();
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
				if (mode != PrisonerInteractionModeDefOf.Execution && !talkee.Awake())
				{
					return true;
				}
				if (!talkee.IsPrisonerOfColony)
				{
					return true;
				}
				return (talkee.guest == null || talkee.guest.interactionMode != mode) ? true : false;
			});
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		public static Toil WaitToBeAbleToInteract(Pawn pawn)
		{
			return new Toil
			{
				initAction = delegate
				{
					if (!pawn.interactions.InteractedTooRecentlyToInteract())
					{
						pawn.jobs.curDriver.ReadyForNextToil();
					}
				},
				tickAction = delegate
				{
					if (!pawn.interactions.InteractedTooRecentlyToInteract())
					{
						pawn.jobs.curDriver.ReadyForNextToil();
					}
				},
				socialMode = RandomSocialMode.Off,
				defaultCompleteMode = ToilCompleteMode.Never
			};
		}

		public static Toil ConvinceRecruitee(Pawn pawn, Pawn talkee)
		{
			Toil obj = new Toil
			{
				initAction = delegate
				{
					if (!pawn.interactions.TryInteractWith(talkee, InteractionDefOf.BuildRapport))
					{
						pawn.jobs.curDriver.ReadyForNextToil();
					}
					else
					{
						pawn.records.Increment(RecordDefOf.PrisonersChatted);
					}
				}
			};
			obj.FailOn(() => !talkee.guest.ScheduledForInteraction);
			obj.socialMode = RandomSocialMode.Off;
			obj.defaultCompleteMode = ToilCompleteMode.Delay;
			obj.defaultDuration = 350;
			return obj;
		}

		public static Toil SetLastInteractTime(TargetIndex targetInd)
		{
			Toil toil = new Toil();
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
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(recruiteeInd).Thing;
				if (pawn.Spawned && pawn.Awake())
				{
					InteractionDef intDef = pawn.AnimalOrWildMan() ? InteractionDefOf.TameAttempt : InteractionDefOf.RecruitAttempt;
					actor.interactions.TryInteractWith(pawn, intDef);
				}
			};
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 350;
			return toil;
		}

		public static Toil TryTrain(TargetIndex traineeInd)
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(traineeInd).Thing;
				if (pawn.Spawned && pawn.Awake() && actor.interactions.TryInteractWith(pawn, InteractionDefOf.TrainAttempt))
				{
					float statValue = actor.GetStatValue(StatDefOf.TrainAnimalChance);
					statValue *= GenMath.LerpDouble(0f, 1f, 1.5f, 0.5f, pawn.RaceProps.wildness);
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
			return toil;
		}

		public static Toil Interact(TargetIndex otherPawnInd, InteractionDef interaction)
		{
			Toil toil = new Toil();
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
	}
}
