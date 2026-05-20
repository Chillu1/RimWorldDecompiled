using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public abstract class JobGiver_AIFightEnemy : ThinkNode_JobGiver
{
	protected float targetAcquireRadius = 56f;

	protected float targetKeepRadius = 65f;

	private bool needLOSToAcquireNonPawnTargets;

	protected bool chaseTarget;

	protected bool allowTurrets;

	protected bool ignoreNonCombatants;

	protected bool humanlikesOnly;

	public static readonly IntRange ExpiryInterval_ShooterSucceeded = new IntRange(450, 550);

	private const int MinTargetDistanceToMove = 5;

	protected virtual bool OnlyUseAbilityVerbs => false;

	protected virtual bool DisableAbilityVerbs => false;

	protected virtual bool OnlyUseRangedSearch => false;

	protected virtual IntRange ExpiryInterval_Melee => new IntRange(360, 480);

	protected virtual IntRange ExpiryInterval_Ability => new IntRange(30, 30);

	protected virtual int TicksSinceEngageToLoseTarget => 400;

	protected abstract bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest, Verb verbToUse = null);

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
		if (pawn.IsColonyMechPlayerControlled && target.Faction == Faction.OfPlayer)
		{
			return false;
		}
		if (humanlikesOnly && target is Pawn pawn2 && !pawn2.RaceProps.Humanlike)
		{
			return false;
		}
		return true;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AIFightEnemy obj = (JobGiver_AIFightEnemy)base.DeepCopy(resolve);
		obj.targetAcquireRadius = targetAcquireRadius;
		obj.targetKeepRadius = targetKeepRadius;
		obj.needLOSToAcquireNonPawnTargets = needLOSToAcquireNonPawnTargets;
		obj.chaseTarget = chaseTarget;
		obj.allowTurrets = allowTurrets;
		obj.ignoreNonCombatants = ignoreNonCombatants;
		obj.humanlikesOnly = humanlikesOnly;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if ((pawn.IsColonist || pawn.IsColonySubhuman) && pawn.playerSettings.hostilityResponse != HostilityResponseMode.Attack && (!(pawn.GetLord()?.LordJob is LordJob_Ritual_Duel lordJob_Ritual_Duel) || !lordJob_Ritual_Duel.duelists.Contains(pawn)))
		{
			return null;
		}
		UpdateEnemyTarget(pawn);
		Thing enemyTarget = pawn.mindState.enemyTarget;
		if (enemyTarget == null)
		{
			return null;
		}
		if (enemyTarget is Pawn pawn2 && pawn2.IsPsychologicallyInvisible())
		{
			return null;
		}
		bool flag = !pawn.IsColonist && !pawn.IsColonySubhuman && !DisableAbilityVerbs;
		if (flag)
		{
			Job abilityJob = GetAbilityJob(pawn, enemyTarget);
			if (abilityJob != null)
			{
				return abilityJob;
			}
		}
		if (OnlyUseAbilityVerbs)
		{
			if (!TryFindShootingPosition(pawn, out var dest))
			{
				return null;
			}
			if (dest == pawn.Position)
			{
				pawn.pather?.StopDead();
				return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_Ability.RandomInRange, checkOverrideOnExpiry: true);
			}
			Job job = JobMaker.MakeJob(JobDefOf.Goto, dest);
			job.expiryInterval = ExpiryInterval_Ability.RandomInRange;
			job.checkOverrideOnExpire = true;
			return job;
		}
		Verb verb = pawn.TryGetAttackVerb(enemyTarget, flag, allowTurrets);
		if (verb == null)
		{
			return null;
		}
		if (verb.verbProps.IsMeleeAttack)
		{
			return MeleeAttackJob(pawn, enemyTarget);
		}
		bool num = CoverUtility.CalculateOverallBlockChance(pawn, enemyTarget.Position, pawn.Map) > 0.01f;
		bool flag2 = pawn.Position.WalkableBy(pawn.Map, pawn) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted);
		bool flag3 = verb.CanHitTarget(enemyTarget);
		bool flag4 = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
		if ((num && flag2 && flag3) || (flag4 && flag3))
		{
			pawn.pather?.StopDead();
			return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
		}
		if (!TryFindShootingPosition(pawn, out var dest2))
		{
			return null;
		}
		if (dest2 == pawn.Position)
		{
			pawn.pather?.StopDead();
			return JobMaker.MakeJob(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, checkOverrideOnExpiry: true);
		}
		Job job2 = JobMaker.MakeJob(JobDefOf.Goto, dest2);
		job2.expiryInterval = ExpiryInterval_ShooterSucceeded.RandomInRange;
		job2.checkOverrideOnExpire = true;
		return job2;
	}

	public static Job GetAbilityJob(Pawn pawn, Thing enemyTarget)
	{
		if (pawn.abilities == null)
		{
			return null;
		}
		List<Ability> list = pawn.abilities.AICastableAbilities(enemyTarget, offensive: true);
		if (list.NullOrEmpty())
		{
			return null;
		}
		if (pawn.Position.WalkableBy(pawn.Map, pawn) && pawn.Map.pawnDestinationReservationManager.CanReserve(pawn.Position, pawn, pawn.Drafted))
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].verb.CanHitTarget(enemyTarget))
				{
					return list[i].GetJob(enemyTarget, enemyTarget);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				LocalTargetInfo localTargetInfo = list[j].AIGetAOETarget();
				if (localTargetInfo.IsValid)
				{
					return list[j].GetJob(localTargetInfo, localTargetInfo);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				if (list[k].verb.targetParams.canTargetSelf)
				{
					return list[k].GetJob(pawn, pawn);
				}
			}
		}
		return null;
	}

	protected virtual Job MeleeAttackJob(Pawn pawn, Thing enemyTarget)
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
		if (thing != null && ShouldLoseTarget(pawn))
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
		if (thing is Pawn pawn2 && thing.Faction == Faction.OfPlayer && pawn.Position.InHorDistOf(thing.Position, 40f) && !pawn2.IsShambler && !pawn.IsPsychologicallyInvisible())
		{
			Find.TickManager.slower.SignalForceNormalSpeed();
		}
	}

	protected Thing FindAttackTargetIfPossible(Pawn pawn)
	{
		if (pawn.TryGetAttackVerb(null, !pawn.IsColonist) == null)
		{
			return null;
		}
		return FindAttackTarget(pawn);
	}

	protected virtual bool ShouldLoseTarget(Pawn pawn)
	{
		Thing enemyTarget = pawn.mindState.enemyTarget;
		if (!enemyTarget.Destroyed && Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick <= TicksSinceEngageToLoseTarget && pawn.CanReach(enemyTarget, PathEndMode.Touch, Danger.Deadly) && !((float)(pawn.Position - enemyTarget.Position).LengthHorizontalSquared > targetKeepRadius * targetKeepRadius))
		{
			return (enemyTarget as IAttackTarget)?.ThreatDisabled(pawn) ?? false;
		}
		return true;
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
		if (ignoreNonCombatants)
		{
			targetScanFlags |= TargetScanFlags.IgnoreNonCombatants;
		}
		return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, (Thing x) => ExtraTargetValidator(pawn, x), 0f, targetAcquireRadius, GetFlagPosition(pawn), GetFlagRadius(pawn), canBashDoors: false, canTakeTargetsCloserThanEffectiveMinRange: true, canBashFences: false, OnlyUseRangedSearch);
	}

	private bool PrimaryVerbIsIncendiary(Pawn pawn)
	{
		if (pawn.equipment?.Primary != null)
		{
			List<Verb> allVerbs = pawn.equipment.Primary.GetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				if (allVerbs[i].verbProps.isPrimary)
				{
					return allVerbs[i].IsIncendiary_Ranged();
				}
			}
		}
		return false;
	}
}
