using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class BreachingUtility
{
	private class BreachRangedCastPositionFinder
	{
		private BreachingGrid breachingGrid;

		private Verb verb;

		private Thing target;

		private bool wouldPutSomeoneElseInDanger;

		private Action<IntVec3> visitDangerousCellFunc;

		private Func<IntVec3, bool> safeForRangedCastFunc;

		public BreachRangedCastPositionFinder()
		{
			visitDangerousCellFunc = VisitDangerousCell;
			safeForRangedCastFunc = SafeForRangedCast;
		}

		private void VisitDangerousCell(IntVec3 cell)
		{
			if (breachingGrid.MarkerGrid[cell] == 180)
			{
				wouldPutSomeoneElseInDanger = true;
			}
		}

		private bool SafeForRangedCast(IntVec3 c)
		{
			if (!SafeUseableFiringPosition(breachingGrid, c))
			{
				return false;
			}
			wouldPutSomeoneElseInDanger = false;
			breachingGrid.VisitDangerousCellsOfAttack(c, target.Position, verb, visitDangerousCellFunc);
			return !wouldPutSomeoneElseInDanger;
		}

		public bool TryFindRangedCastPosition(Pawn pawn, Verb verb, Thing target, out IntVec3 result)
		{
			try
			{
				result = IntVec3.Invalid;
				LordToilData_AssaultColonyBreaching lordToilData_AssaultColonyBreaching = LordDataFor(pawn.GetLord());
				if (lordToilData_AssaultColonyBreaching == null)
				{
					return false;
				}
				breachingGrid = lordToilData_AssaultColonyBreaching.breachingGrid;
				this.verb = verb;
				this.target = target;
				CastPositionRequest newReq = new CastPositionRequest
				{
					caster = pawn,
					target = target,
					verb = verb,
					maxRangeFromTarget = verb.EffectiveRange
				};
				if (lordToilData_AssaultColonyBreaching.soloAttacker == null)
				{
					newReq.maxRangeFromTarget = Mathf.Min(lordToilData_AssaultColonyBreaching.maxRange, newReq.maxRangeFromTarget);
				}
				newReq.validator = safeForRangedCastFunc;
				if (CastPositionFinder.TryFindCastPosition(newReq, out var dest))
				{
					result = dest;
					return true;
				}
				return false;
			}
			finally
			{
			}
		}
	}

	public static readonly IntRange TrashJobCheckOverrideInterval = new IntRange(120, 300);

	private static readonly FloatRange EscortRadiusRanged = new FloatRange(15f, 19f);

	private static readonly FloatRange EscortRadiusMelee = new FloatRange(23f, 26f);

	private static BreachRangedCastPositionFinder cachedRangedCastPositionFinder = new BreachRangedCastPositionFinder();

	public static bool ShouldBreachBuilding(Thing thing)
	{
		if (!(thing is Building building))
		{
			return false;
		}
		if (!TrashUtility.ShouldTrashBuilding(building) || !PathUtility.IsDestroyable(building))
		{
			return false;
		}
		if (!building.def.IsEdifice() || building.def.IsFrame)
		{
			return false;
		}
		if (building.def.mineable)
		{
			return true;
		}
		if ((building.Faction == null || building.Faction == Faction.OfPlayer) && BlocksBreaching(building.Map, building.Position))
		{
			return true;
		}
		return false;
	}

	public static bool IsWorthBreachingBuilding(BreachingGrid grid, Building b)
	{
		if (b.def.passability != Traversability.Impassable || b.def.IsDoor)
		{
			return true;
		}
		if (OnEdgeOfPath(b.Map, grid.BreachGrid, b.Position))
		{
			if (OnEdgeOfPathAndSufficientlyClear(grid, b.Position, IntVec3.North) || OnEdgeOfPathAndSufficientlyClear(grid, b.Position, IntVec3.East) || OnEdgeOfPathAndSufficientlyClear(grid, b.Position, IntVec3.South) || OnEdgeOfPathAndSufficientlyClear(grid, b.Position, IntVec3.West))
			{
				return false;
			}
		}
		else if (!AnyAdjacentImpassibleOnBreachPath(b.Map, grid.BreachGrid, b.Position))
		{
			return false;
		}
		return true;
	}

	public static int CountReachableAdjacentCells(BreachingGrid grid, Building b)
	{
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			IntVec3 c = b.Position + GenAdj.CardinalDirections[i];
			if (c.InBounds(grid.Map) && grid.ReachableGrid[c])
			{
				num++;
			}
		}
		return num;
	}

	private static bool OnEdgeOfPath(Map map, BoolGrid grid, IntVec3 p)
	{
		for (int i = 0; i < 4; i++)
		{
			IntVec3 c = p + GenAdj.CardinalDirections[i];
			if (c.InBounds(map) && !grid[c])
			{
				return true;
			}
		}
		return false;
	}

	private static bool OnEdgeOfPathAndSufficientlyClear(BreachingGrid breachingGrid, IntVec3 p, IntVec3 right)
	{
		Map map = breachingGrid.Map;
		BoolGrid breachGrid = breachingGrid.BreachGrid;
		int num = breachingGrid.BreachRadius * 2 + 1;
		IntVec3 c = p - right;
		if (!c.InBounds(map) || breachGrid[c])
		{
			return false;
		}
		IntVec3 c2 = p;
		int num2 = 0;
		while (c2.InBounds(map) && breachGrid[c2] && BlocksBreaching(map, c2) && num2 <= num)
		{
			c2 += right;
			num2++;
		}
		int num3 = 0;
		while (c2.InBounds(map) && breachGrid[c2] && !BlocksBreaching(map, c2) && num2 <= num)
		{
			c2 += right;
			num2++;
			num3++;
		}
		int num4 = Math.Max(2, num - 2);
		return num3 >= num4;
	}

	public static bool BlocksBreaching(Map map, IntVec3 c)
	{
		Building edifice = c.GetEdifice(map);
		if (edifice != null && edifice.def.building.ai_neverTrashThis)
		{
			return false;
		}
		if (!c.Impassable(map))
		{
			return c.GetDoor(map) != null;
		}
		return true;
	}

	private static bool AnyAdjacentImpassibleOnBreachPath(Map map, BoolGrid grid, IntVec3 position)
	{
		for (int i = 0; i < 8; i++)
		{
			IntVec3 c = position + GenAdj.AdjacentCellsAround[i];
			if (c.InBounds(map) && grid[c] && BlocksBreaching(map, c))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanSoloAttackTargetBuilding(Thing thing)
	{
		if (thing == null)
		{
			return false;
		}
		if (thing.Faction == Faction.OfPlayer)
		{
			return true;
		}
		return false;
	}

	public static bool IsSoloAttackVerb(Verb verb)
	{
		return verb.verbProps.ai_AvoidFriendlyFireRadius > 0f;
	}

	public static float EscortRadius(Pawn pawn)
	{
		if (pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
		{
			return EscortRadiusRanged.RandomInRange;
		}
		return EscortRadiusMelee.RandomInRange;
	}

	private static bool UsableVerb(Verb verb)
	{
		if (verb == null || !verb.Available())
		{
			return false;
		}
		if (!verb.HarmsHealth())
		{
			return false;
		}
		return true;
	}

	public static Verb FindVerbToUseForBreaching(Pawn pawn)
	{
		CompEquippable compEquippable = pawn.equipment?.PrimaryEq;
		if (compEquippable == null)
		{
			return null;
		}
		Verb primaryVerb = compEquippable.PrimaryVerb;
		if (UsableVerb(primaryVerb) && primaryVerb.verbProps.ai_IsBuildingDestroyer)
		{
			return primaryVerb;
		}
		List<Verb> allVerbs = compEquippable.AllVerbs;
		for (int i = 0; i < allVerbs.Count; i++)
		{
			Verb verb = allVerbs[i];
			if (UsableVerb(verb) && verb.verbProps.ai_IsBuildingDestroyer)
			{
				return verb;
			}
		}
		if (UsableVerb(primaryVerb))
		{
			return primaryVerb;
		}
		return null;
	}

	public static void FinalizeTrashJob(Job job)
	{
		job.expiryInterval = TrashJobCheckOverrideInterval.RandomInRange;
		job.checkOverrideOnExpire = true;
		job.expireRequiresEnemiesNearby = true;
	}

	public static BreachingGrid BreachingGridFor(Pawn pawn)
	{
		return BreachingGridFor(pawn?.GetLord());
	}

	public static BreachingGrid BreachingGridFor(Lord lord)
	{
		return LordDataFor(lord)?.breachingGrid;
	}

	public static LordToilData_AssaultColonyBreaching LordDataFor(Lord lord)
	{
		return (lord?.CurLordToil as LordToil_AssaultColonyBreaching)?.Data;
	}

	public static LordToil_AssaultColonyBreaching LordToilOf(Pawn pawn)
	{
		return pawn?.GetLord().CurLordToil as LordToil_AssaultColonyBreaching;
	}

	public static bool TryFindCastPosition(Pawn pawn, Verb verb, Thing target, out IntVec3 result)
	{
		if (verb.IsMeleeAttack)
		{
			return TryFindMeleeCastPosition(pawn, verb, target, out result);
		}
		return cachedRangedCastPositionFinder.TryFindRangedCastPosition(pawn, verb, target, out result);
	}

	private static bool TryFindMeleeCastPosition(Pawn pawn, Verb verb, Thing target, out IntVec3 result)
	{
		result = IntVec3.Invalid;
		BreachingGrid breachingGrid = BreachingGridFor(pawn);
		if (breachingGrid == null)
		{
			return false;
		}
		if (SafeUseableFiringPosition(breachingGrid, pawn.Position) && pawn.CanReachImmediate(target, PathEndMode.Touch))
		{
			result = pawn.Position;
			return true;
		}
		for (int i = 0; i < 8; i++)
		{
			IntVec3 intVec = GenAdj.AdjacentCells[i] + target.Position;
			if (intVec.InBounds(target.Map) && SafeUseableFiringPosition(breachingGrid, intVec) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly))
			{
				result = intVec;
				return true;
			}
		}
		return false;
	}

	private static bool SafeUseableFiringPosition(BreachingGrid grid, IntVec3 c)
	{
		if (!grid.ReachableGrid[c])
		{
			return false;
		}
		if (grid.MarkerGrid[c] == 180)
		{
			return false;
		}
		if (c.ContainsStaticFire(grid.Map))
		{
			return false;
		}
		return true;
	}

	public static Pawn FindPawnToEscort(Pawn follower)
	{
		Lord lord = follower.GetLord();
		if (lord == null)
		{
			return null;
		}
		Pawn result = null;
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			if (pawn != follower && pawn.mindState.duty?.def == DutyDefOf.Breaching)
			{
				BreachingTargetData breachingTarget = pawn.mindState.breachingTarget;
				if (breachingTarget != null && breachingTarget.firingPosition.IsValid)
				{
					return pawn;
				}
				result = pawn;
			}
		}
		return result;
	}

	public static bool CanDamageTarget(Verb verb, Thing target)
	{
		if (verb.GetDamageDef() == DamageDefOf.Flame && !target.FlammableNow)
		{
			return false;
		}
		return true;
	}

	public static bool IsGoodBreacher(Pawn pawn)
	{
		return FindVerbToUseForBreaching(pawn)?.verbProps.ai_IsBuildingDestroyer ?? false;
	}
}
