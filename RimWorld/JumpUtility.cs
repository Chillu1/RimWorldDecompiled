using Verse;
using Verse.AI;

namespace RimWorld;

public static class JumpUtility
{
	public static bool DoJump(Pawn pawn, LocalTargetInfo currentTarget, CompApparelReloadable comp, VerbProperties verbProps, Ability triggeringAbility = null, LocalTargetInfo target = default(LocalTargetInfo), ThingDef pawnFlyerOverride = null)
	{
		if (comp != null && !comp.CanBeUsed(out var _))
		{
			return false;
		}
		comp?.UsedOnce();
		IntVec3 position = pawn.Position;
		IntVec3 cell = currentTarget.Cell;
		Map map = pawn.Map;
		bool flag = Find.Selector.IsSelected(pawn);
		PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(pawnFlyerOverride ?? ThingDefOf.PawnFlyer, pawn, cell, verbProps.flightEffecterDef, verbProps.soundLanding, verbProps.flyWithCarriedThing, null, triggeringAbility, target);
		if (pawnFlyer != null)
		{
			FleckMaker.ThrowDustPuff(position.ToVector3Shifted() + Gen.RandomHorizontalVector(0.5f), map, 2f);
			GenSpawn.Spawn(pawnFlyer, cell, map);
			if (flag)
			{
				Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
			}
			return true;
		}
		return false;
	}

	public static void OrderJump(Pawn pawn, LocalTargetInfo target, Verb verb, float range)
	{
		Map map = pawn.Map;
		IntVec3 intVec = RCellFinder.BestOrderedGotoDestNear(target.Cell, pawn, (IntVec3 c) => ValidJumpTarget(pawn, map, c) && CanHitTargetFrom(pawn, pawn.Position, c, range), reachable: false);
		Job job = JobMaker.MakeJob(JobDefOf.CastJump, intVec);
		job.verbToUse = verb;
		if (pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc))
		{
			FleckMaker.Static(intVec, map, FleckDefOf.FeedbackGoto);
		}
	}

	public static bool CanHitTargetFrom(Pawn pawn, IntVec3 root, LocalTargetInfo targ, float range)
	{
		float num = range * range;
		IntVec3 cell = targ.Cell;
		if ((float)pawn.Position.DistanceToSquared(cell) <= num)
		{
			return GenSight.LineOfSight(root, cell, pawn.Map);
		}
		return false;
	}

	public static bool ValidJumpTarget(Thing flying, Map map, IntVec3 cell)
	{
		if (!cell.IsValid || !cell.InBounds(map))
		{
			return false;
		}
		if (cell.Impassable(map) || cell.Fogged(map))
		{
			return false;
		}
		if (flying is Pawn pawn)
		{
			if (!cell.WalkableBy(map, pawn))
			{
				return false;
			}
		}
		else if (!cell.WalkableByAny(map))
		{
			return false;
		}
		Building edifice = cell.GetEdifice(map);
		if (edifice != null && edifice is Building_Door { Open: false })
		{
			return false;
		}
		return true;
	}
}
