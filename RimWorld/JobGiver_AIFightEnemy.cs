using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class JobGiver_AIFightEnemy : ThinkNode_JobGiver
	{
		private float targetAcquireRadius = 56f;

		private float targetKeepRadius = 65f;

		private bool needLOSToAcquireNonPawnTargets;

		private bool chaseTarget;

		public static readonly IntRange ExpiryInterval_ShooterSucceeded = new IntRange(450, 550);

		private static readonly IntRange ExpiryInterval_Melee = new IntRange(360, 480);

		private const int MinTargetDistanceToMove = 5;

		private const int TicksSinceEngageToLoseTarget = 400;

		protected abstract bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest);

		protected virtual float GetFlagRadius(Pawn pawn)
		{
			return 999999f;
		}

		protected virtual IntVec3 GetFlagPosition(Pawn pawn)
		{
			return IntVec3.Invalid;
		}

		protected virtual bool ExtraTargetValidator(Pawn pawn, Thing target)
		{
			return true;
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AIFightEnemy obj = (JobGiver_AIFightEnemy)base.DeepCopy(resolve);
			obj.targetAcquireRadius = targetAcquireRadius;
			obj.targetKeepRadius = targetKeepRadius;
			obj.needLOSToAcquireNonPawnTargets = needLOSToAcquireNonPawnTargets;
			obj.chaseTarget = chaseTarget;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			UpdateEnemyTarget(pawn);
			Thing enemyTarget = pawn.mindState.enemyTarget;
			if (enemyTarget == null)
			{
				return null;
			}
			Pawn pawn2 = enemyTarget as Pawn;
			if (pawn2 != null && pawn2.IsInvisible())
			{
				return null;
			}
			bool allowManualCastWeapons = !pawn.IsColonist;
			Verb verb = pawn.TryGetAttackVerb(enemyTarget, allowManualCastWeapons);
			if (verb == null)
			{
				return null;
			}
			if (verb.verbProps.IsMeleeAttack)
			{
				return MeleeAttackJob(enemyTarget);
			}
			bool num = CoverUtility.CalculateOverallBlockChance(pawn, enemyTarget.Position, pawn.Map) > 0.01f;
			bool flag = pawn.Position.Standable(pawn.Map) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
			bool flag2 = verb.CanHitTarget(enemyTarget);
			bool flag3 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
			if ((num && flag && flag2) || (flag3 && flag2))
			{
				return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
			}
			if (!TryFindShootingPosition(pawn, out var dest))
			{
				return null;
			}
			if (dest == pawn.Position)
			{
				return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
			}
			Job job = JobMaker.MakeJob(JobDefOf.Goto, dest);
			job.expiryInterval = ExpiryInterval_ShooterSucceeded.RandomInRange;
			job.checkOverrideOnExpire = true;
			return job;
		}

		protected virtual Job MeleeAttackJob(Thing enemyTarget)
		{
			Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, enemyTarget);
			job.expiryInterval = ExpiryInterval_Melee.RandomInRange;
			job.checkOverrideOnExpire = true;
			job.expireRequiresEnemiesNearby = true;
			return job;
		}

		protected virtual void UpdateEnemyTarget(Pawn pawn)
		{
			Thing thing = pawn.mindState.enemyTarget;
			if (thing != null && (thing.Destroyed || Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 || !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly) || (float)(pawn.Position - thing.Position).LengthHorizontalSquared > targetKeepRadius * targetKeepRadius || ((IAttackTarget)thing).ThreatDisabled(pawn)))
			{
				thing = null;
			}
			if (thing == null)
			{
				thing = FindAttackTargetIfPossible(pawn);
				if (thing != null)
				{
					pawn.mindState.Notify_EngagedTarget();
					pawn.GetLord()?.Notify_PawnAcquiredTarget(pawn, thing);
				}
			}
			else
			{
				Thing thing2 = FindAttackTargetIfPossible(pawn);
				if (thing2 == null && !chaseTarget)
				{
					thing = null;
				}
				else if (thing2 != null && thing2 != thing)
				{
					pawn.mindState.Notify_EngagedTarget();
					thing = thing2;
				}
			}
			pawn.mindState.enemyTarget = thing;
			if (thing is Pawn && thing.Faction == Faction.OfPlayer && pawn.Position.InHorDistOf(thing.Position, 40f))
			{
				Find.TickManager.slower.SignalForceNormalSpeed();
			}
		}

		private Thing FindAttackTargetIfPossible(Pawn pawn)
		{
			if (pawn.TryGetAttackVerb(null, !pawn.IsColonist) == null)
			{
				return null;
			}
			return FindAttackTarget(pawn);
		}

		protected virtual Thing FindAttackTarget(Pawn pawn)
		{
			TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
			if (needLOSToAcquireNonPawnTargets)
			{
				targetScanFlags |= TargetScanFlags.NeedLOSToNonPawns;
			}
			if (PrimaryVerbIsIncendiary(pawn))
			{
				targetScanFlags |= TargetScanFlags.NeedNonBurning;
			}
			return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, (Thing x) => ExtraTargetValidator(pawn, x), 0f, targetAcquireRadius, GetFlagPosition(pawn), GetFlagRadius(pawn));
		}

		private bool PrimaryVerbIsIncendiary(Pawn pawn)
		{
			if (pawn.equipment != null && pawn.equipment.Primary != null)
			{
				List<Verb> allVerbs = pawn.equipment.Primary.GetComp<CompEquippable>().AllVerbs;
				for (int i = 0; i < allVerbs.Count; i++)
				{
					if (allVerbs[i].verbProps.isPrimary)
					{
						return allVerbs[i].IsIncendiary();
					}
				}
			}
			return false;
		}
	}
}
