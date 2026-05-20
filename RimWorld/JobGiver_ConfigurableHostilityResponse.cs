using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_ConfigurableHostilityResponse : ThinkNode_JobGiver
{
	private static List<Thing> tmpThreats = new List<Thing>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.playerSettings == null || !pawn.playerSettings.UsesConfigurableHostilityResponse)
		{
			return null;
		}
		if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
		{
			return null;
		}
		if (pawn.Downed)
		{
			return null;
		}
		if (ModsConfig.AnomalyActive && pawn.GetLord()?.LordJob is LordJob_PsychicRitual)
		{
			return null;
		}
		return pawn.playerSettings.hostilityResponse switch
		{
			HostilityResponseMode.Ignore => null, 
			HostilityResponseMode.Attack => TryGetAttackNearbyEnemyJob(pawn), 
			HostilityResponseMode.Flee => TryGetFleeJob(pawn), 
			_ => null, 
		};
	}

	private Job TryGetAttackNearbyEnemyJob(Pawn pawn)
	{
		if (pawn.WorkTagIsDisabled(WorkTags.Violent))
		{
			return null;
		}
		bool isMeleeAttack = pawn.CurrentEffectiveVerb.IsMeleeAttack;
		float maxDist = 8f;
		if (!isMeleeAttack)
		{
			maxDist = Mathf.Clamp(pawn.CurrentEffectiveVerb.EffectiveRange * 0.66f, 2f, 20f);
		}
		Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, null, 0f, maxDist);
		if (thing == null)
		{
			return null;
		}
		if (isMeleeAttack || pawn.CanReachImmediate(thing, PathEndMode.Touch))
		{
			return JobMaker.MakeJob(JobDefOf.AttackMelee, thing);
		}
		Verb verb = pawn.TryGetAttackVerb(thing, !pawn.IsColonist);
		if (verb == null || verb.ApparelPreventsShooting())
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.AttackStatic, thing);
		job.maxNumStaticAttacks = 2;
		job.expiryInterval = 2000;
		job.endIfCantShootTargetFromCurPos = true;
		return job;
	}

	private Job TryGetFleeJob(Pawn pawn)
	{
		if (!SelfDefenseUtility.ShouldStartFleeing(pawn))
		{
			return null;
		}
		IntVec3 intVec;
		if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.FleeAndCower)
		{
			intVec = pawn.CurJob.targetA.Cell;
		}
		else
		{
			tmpThreats.Clear();
			List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				Thing thing = potentialTargetsFor[i].Thing;
				if (FleeUtility.ShouldFleeFrom(thing, pawn, checkDistance: false, checkLOS: false))
				{
					tmpThreats.Add(thing);
				}
			}
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
			for (int j = 0; j < list.Count; j++)
			{
				Thing thing2 = list[j];
				if (FleeUtility.ShouldFleeFrom(thing2, pawn, checkDistance: false, checkLOS: false))
				{
					tmpThreats.Add(thing2);
				}
			}
			if (!tmpThreats.Any())
			{
				Log.Error(pawn.LabelShort + " decided to flee but there is not any threat around.");
				Region region = pawn.GetRegion();
				if (region == null)
				{
					return null;
				}
				RegionTraverser.BreadthFirstTraverse(region, (Region from, Region reg) => reg.door == null || reg.door.Open, delegate(Region reg)
				{
					List<Thing> list2 = reg.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
					for (int k = 0; k < list2.Count; k++)
					{
						Thing thing3 = list2[k];
						if (FleeUtility.ShouldFleeFrom(thing3, pawn, checkDistance: false, checkLOS: false))
						{
							tmpThreats.Add(thing3);
							Log.Warning($"  Found a viable threat {thing3.LabelShort}; tests are {thing3.Map.attackTargetsCache.Debug_CheckIfInAllTargets(thing3 as IAttackTarget)}, {thing3.Map.attackTargetsCache.Debug_CheckIfHostileToFaction(pawn.Faction, thing3 as IAttackTarget)}, {thing3 is IAttackTarget}");
						}
					}
					return false;
				}, 9);
				if (!tmpThreats.Any())
				{
					return null;
				}
			}
			intVec = CellFinderLoose.GetFleeDest(pawn, tmpThreats);
			tmpThreats.Clear();
		}
		return JobMaker.MakeJob(JobDefOf.FleeAndCower, intVec);
	}
}
