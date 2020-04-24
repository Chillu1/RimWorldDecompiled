using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
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
			switch (pawn.playerSettings.hostilityResponse)
			{
			case HostilityResponseMode.Ignore:
				return null;
			case HostilityResponseMode.Attack:
				return TryGetAttackNearbyEnemyJob(pawn);
			case HostilityResponseMode.Flee:
				return TryGetFleeJob(pawn);
			default:
				return null;
			}
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
				maxDist = Mathf.Clamp(pawn.CurrentEffectiveVerb.verbProps.range * 0.66f, 2f, 20f);
			}
			Thing thing = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, null, 0f, maxDist);
			if (thing == null)
			{
				return null;
			}
			if (isMeleeAttack || pawn.CanReachImmediate(thing, PathEndMode.Touch))
			{
				return JobMaker.MakeJob(JobDefOf.AttackMelee, thing);
			}
			Verb verb = pawn.TryGetAttackVerb(thing, !pawn.IsColonist);
			if (verb == null || verb.ApparelPreventsShooting(pawn.Position, thing))
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
			IntVec3 c;
			if (pawn.CurJob != null && pawn.CurJob.def == JobDefOf.FleeAndCower)
			{
				c = pawn.CurJob.targetA.Cell;
			}
			else
			{
				tmpThreats.Clear();
				List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
				for (int i = 0; i < potentialTargetsFor.Count; i++)
				{
					Thing thing = potentialTargetsFor[i].Thing;
					if (SelfDefenseUtility.ShouldFleeFrom(thing, pawn, checkDistance: false, checkLOS: false))
					{
						tmpThreats.Add(thing);
					}
				}
				List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.AlwaysFlee);
				for (int j = 0; j < list.Count; j++)
				{
					Thing thing2 = list[j];
					if (SelfDefenseUtility.ShouldFleeFrom(thing2, pawn, checkDistance: false, checkLOS: false))
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
							if (SelfDefenseUtility.ShouldFleeFrom(thing3, pawn, checkDistance: false, checkLOS: false))
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
				c = CellFinderLoose.GetFleeDest(pawn, tmpThreats);
				tmpThreats.Clear();
			}
			return JobMaker.MakeJob(JobDefOf.FleeAndCower, c);
		}
	}
}
