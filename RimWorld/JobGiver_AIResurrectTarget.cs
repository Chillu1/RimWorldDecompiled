using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AIResurrectTarget : ThinkNode_JobGiver
{
	private AbilityDef abilityDef;

	private int expiryInterval = 500;

	private int maxRegions = 50;

	private static readonly HashSet<Corpse> tmpReservedCorpses = new HashSet<Corpse>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		using (new ProfilerBlock("JobGiver_AIResurrectTarget.TryGiveJob"))
		{
			Ability ability = pawn.abilities.GetAbility(abilityDef);
			if (ability == null || !ability.CanCast)
			{
				return null;
			}
			if (!pawn.Spawned)
			{
				return null;
			}
			UpdateResurrectTarget(pawn);
			if (pawn.mindState.resurrectTarget == null)
			{
				return null;
			}
			Job job = ability.GetJob(pawn.mindState.resurrectTarget.corpse, pawn.mindState.resurrectTarget.castPosition);
			job.expiryInterval = expiryInterval;
			return job;
		}
	}

	private static HashSet<Corpse> ReservedCorpsesForResurrection(Map map, Faction faction)
	{
		using (new ProfilerBlock("ReservedCorpsesForResurrection"))
		{
			tmpReservedCorpses.Clear();
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
			for (int i = 0; i < list.Count; i++)
			{
				Corpse corpse = list[i].mindState?.resurrectTarget?.corpse;
				if (corpse != null)
				{
					tmpReservedCorpses.Add(corpse);
				}
			}
			return tmpReservedCorpses;
		}
	}

	private void UpdateResurrectTarget(Pawn pawn)
	{
		pawn.mindState.resurrectTarget = null;
		Ability ability = pawn.abilities.GetAbility(abilityDef);
		List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse);
		list.SortBy((Thing c) => c.Position.DistanceToSquared(pawn.Position));
		HashSet<Corpse> hashSet = ReservedCorpsesForResurrection(pawn.Map, pawn.Faction);
		for (int num = 0; num < list.Count; num++)
		{
			Corpse corpse = (Corpse)list[num];
			if (hashSet.Contains(corpse) || !ShouldResurrectCorpse(corpse, pawn) || !ability.CanApplyOn(new LocalTargetInfo(corpse)))
			{
				continue;
			}
			using (new ProfilerBlock("TryFindCastPosition"))
			{
				if (CastPositionFinder.TryFindCastPosition(new CastPositionRequest
				{
					caster = pawn,
					target = corpse,
					verb = ability.verb,
					maxRangeFromTarget = ability.verb.EffectiveRange,
					wantCoverFromTarget = false,
					maxRegions = maxRegions
				}, out var dest))
				{
					pawn.mindState.resurrectTarget = new ResurrectCorpseData(corpse, dest);
					break;
				}
			}
		}
	}

	public static bool ShouldResurrectCorpse(Corpse corpse, Pawn pawn)
	{
		if (corpse != null && corpse.Spawned && corpse.Map == pawn.Map && corpse.InnerPawn.Faction == pawn.Faction && pawn.CanReserve(corpse))
		{
			return !corpse.InnerPawn.kindDef.abilities.NotNullAndContains(AbilityDefOf.ResurrectionMech);
		}
		return false;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AIResurrectTarget obj = (JobGiver_AIResurrectTarget)base.DeepCopy(resolve);
		obj.abilityDef = abilityDef;
		obj.expiryInterval = expiryInterval;
		obj.maxRegions = maxRegions;
		return obj;
	}
}
