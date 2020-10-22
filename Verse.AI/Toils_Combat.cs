using System;
using RimWorld;
using UnityEngine;

namespace Verse.AI
{
	public static class Toils_Combat
	{
		public static Toil TrySetJobToUseAttackVerb(TargetIndex targetInd)
		{
			Toil toil = new Toil();
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

		public static Toil GotoCastPosition(TargetIndex targetInd, bool closeIfDowned = false, float maxRangeFactor = 1f)
		{
			Toil toil = new Toil();
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
					CastPositionRequest newReq = default(CastPositionRequest);
					newReq.caster = toil.actor;
					newReq.target = thing;
					newReq.verb = curJob.verbToUse;
					newReq.maxRangeFromTarget = ((!closeIfDowned || pawn == null || !pawn.Downed) ? Mathf.Max(curJob.verbToUse.verbProps.range * maxRangeFactor, 1.42f) : Mathf.Min(curJob.verbToUse.verbProps.range, pawn.RaceProps.executionRange));
					newReq.wantCoverFromTarget = false;
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
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(targetInd);
				LocalTargetInfo destTarg = ((destInd != 0) ? toil.actor.jobs.curJob.GetTarget(destInd) : LocalTargetInfo.Invalid);
				toil.actor.jobs.curJob.verbToUse.TryStartCastOn(target, destTarg, surpriseAttack: false, canHitNonTargetPawns);
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
			Toil followAndAttack = new Toil();
			followAndAttack.tickAction = delegate
			{
				Pawn actor = followAndAttack.actor;
				Job curJob = actor.jobs.curJob;
				JobDriver curDriver = actor.jobs.curDriver;
				Thing thing = curJob.GetTarget(targetInd).Thing;
				Pawn pawn = thing as Pawn;
				if (!thing.Spawned || (pawn != null && pawn.IsInvisible()))
				{
					curDriver.ReadyForNextToil();
				}
				else if (thing != actor.pather.Destination.Thing || (!actor.pather.Moving && !actor.CanReachImmediate(thing, PathEndMode.Touch)))
				{
					actor.pather.StartPath(thing, PathEndMode.Touch);
				}
				else if (actor.CanReachImmediate(thing, PathEndMode.Touch))
				{
					if (pawn != null && pawn.Downed && !curJob.killIncappedTarget)
					{
						curDriver.ReadyForNextToil();
					}
					else
					{
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
