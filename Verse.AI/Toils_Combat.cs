using System;
using RimWorld;
using UnityEngine;

namespace Verse.AI
{
	public static class Toils_Combat
	{
		public static Toil TrySetJobToUseAttackVerb(TargetIndex targetInd)
		{
			Toil toil = ToilMaker.MakeToil("TrySetJobToUseAttackVerb");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				bool allowManualCastWeapons = !actor.IsColonist;
				Verb verb = actor.TryGetAttackVerb(curJob.GetTarget(targetInd).Thing, allowManualCastWeapons);
				if (verb == null)
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else
				{
					curJob.verbToUse = verb;
				}
			};
			return toil;
		}

		public static Toil GotoCastPosition(TargetIndex targetInd, TargetIndex castPositionInd = TargetIndex.None, bool closeIfDowned = false, float maxRangeFactor = 1f)
		{
			Toil toil = ToilMaker.MakeToil("GotoCastPosition");
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Job curJob = actor.jobs.curJob;
				Thing thing = curJob.GetTarget(targetInd).Thing;
				Pawn pawn = thing as Pawn;
				if (actor == thing)
				{
					actor.pather.StopDead();
					actor.jobs.curDriver.ReadyForNextToil();
				}
				else if (thing == null)
				{
					actor.pather.StopDead();
					actor.jobs.curDriver.ReadyForNextToil();
				}
				else
				{
					CastPositionRequest newReq = new CastPositionRequest
					{
						caster = toil.actor,
						target = thing,
						verb = curJob.verbToUse,
						maxRangeFromTarget = ((!closeIfDowned || pawn == null || !pawn.Downed) ? Mathf.Max(curJob.verbToUse.verbProps.range * maxRangeFactor, 1.42f) : Mathf.Min(curJob.verbToUse.verbProps.range, pawn.RaceProps.executionRange)),
						wantCoverFromTarget = false
					};
					if (castPositionInd != TargetIndex.None)
					{
						newReq.preferredCastPosition = curJob.GetTarget(castPositionInd).Cell;
					}
					if (!CastPositionFinder.TryFindCastPosition(newReq, out var dest))
					{
						toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					else
					{
						toil.actor.pather.StartPath(dest, PathEndMode.OnCell);
						actor.Map.pawnDestinationReservationManager.Reserve(actor, curJob, dest);
					}
				}
			};
			toil.FailOnDespawnedOrNull(targetInd);
			toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
			return toil;
		}

		public static Toil CastVerb(TargetIndex targetInd, bool canHitNonTargetPawns = true)
		{
			return CastVerb(targetInd, TargetIndex.None, canHitNonTargetPawns);
		}

		public static Toil CastVerb(TargetIndex targetInd, TargetIndex destInd, bool canHitNonTargetPawns = true)
		{
			Toil toil = ToilMaker.MakeToil("CastVerb");
			toil.initAction = delegate
			{
				LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(targetInd);
				LocalTargetInfo destTarg = ((destInd != TargetIndex.None) ? toil.actor.jobs.curJob.GetTarget(destInd) : LocalTargetInfo.Invalid);
				toil.actor.jobs.curJob.verbToUse.TryStartCastOn(target, destTarg, surpriseAttack: false, canHitNonTargetPawns, toil.actor.jobs.curJob.preventFriendlyFire);
			};
			toil.defaultCompleteMode = ToilCompleteMode.FinishedBusy;
			toil.activeSkill = () => GetActiveSkillForToil(toil);
			return toil;
		}

		public static SkillDef GetActiveSkillForToil(Toil toil)
		{
			Verb verbToUse = toil.actor.jobs.curJob.verbToUse;
			if (verbToUse != null && verbToUse.EquipmentSource != null)
			{
				if (verbToUse.EquipmentSource.def.IsMeleeWeapon)
				{
					return SkillDefOf.Melee;
				}
				if (verbToUse.EquipmentSource.def.IsRangedWeapon)
				{
					return SkillDefOf.Shooting;
				}
			}
			return null;
		}

		public static Toil FollowAndMeleeAttack(TargetIndex targetInd, Action hitAction)
		{
			return FollowAndMeleeAttack(targetInd, TargetIndex.None, hitAction);
		}

		public static Toil FollowAndMeleeAttack(TargetIndex targetInd, TargetIndex standPositionInd, Action hitAction)
		{
			Toil followAndAttack = ToilMaker.MakeToil("FollowAndMeleeAttack");
			followAndAttack.tickIntervalAction = delegate(int delta)
			{
				Pawn actor = followAndAttack.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver curDriver = actor.jobs.curDriver;
				LocalTargetInfo target = curJob.GetTarget(targetInd);
				Thing thing = target.Thing;
				Pawn pawn = thing as Pawn;
				bool flag = actor.IsHashIntervalTick(250, delta);
				if (!thing.Spawned || (pawn != null && pawn.IsPsychologicallyInvisible()) || (flag && !actor.CanReach(thing, PathEndMode.Touch, Danger.Deadly)))
				{
					curDriver.ReadyForNextToil();
				}
				else
				{
					LocalTargetInfo localTargetInfo = target;
					PathEndMode peMode = PathEndMode.Touch;
					if (standPositionInd != TargetIndex.None)
					{
						LocalTargetInfo target2 = curJob.GetTarget(standPositionInd);
						if (target2.IsValid)
						{
							localTargetInfo = target2;
							peMode = PathEndMode.OnCell;
						}
					}
					if (localTargetInfo != actor.pather.Destination || (!actor.pather.Moving && !actor.CanReachImmediate(target, PathEndMode.Touch)))
					{
						if (actor.CurJob.ensureReachable && !actor.CanReach(target, peMode, Danger.Deadly))
						{
							curDriver.ReadyForNextToil();
						}
						else
						{
							actor.pather.StartPath(localTargetInfo, peMode);
						}
					}
					else if (actor.CanReachImmediate(target, PathEndMode.Touch))
					{
						if (pawn != null)
						{
							bool flag2 = pawn.IsMutant && pawn.mutant.Def.canAttackWhileCrawling && !pawn.ThreatDisabled(null);
							if (pawn.Downed && !flag2 && !curJob.killIncappedTarget)
							{
								curDriver.ReadyForNextToil();
								return;
							}
						}
						hitAction();
					}
				}
			};
			followAndAttack.activeSkill = () => SkillDefOf.Melee;
			followAndAttack.defaultCompleteMode = ToilCompleteMode.Never;
			return followAndAttack;
		}
	}
}
