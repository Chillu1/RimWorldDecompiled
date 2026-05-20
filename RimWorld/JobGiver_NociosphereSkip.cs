using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_NociosphereSkip : ThinkNode_JobGiver
{
	private const float TeleportAngleError = 30f;

	private const int RandomJumpRadius = 14;

	private const int PawnClearDistanceTiles = 3;

	private const float MinJumpDist = 6f;

	private const float MaxJumpDist = 19.9f;

	private const Intelligence MinIntelligence = Intelligence.ToolUser;

	private static readonly List<Pawn> targets = new List<Pawn>();

	protected override Job TryGiveJob(Pawn pawn)
	{
		AbilityDef entitySkip = AbilityDefOf.EntitySkip;
		Job curJob = pawn.CurJob;
		if (curJob != null && curJob.def == entitySkip.jobDef)
		{
			return curJob;
		}
		Ability ability = pawn.abilities.GetAbility(AbilityDefOf.EntitySkip);
		if (!ability.CanCast)
		{
			return null;
		}
		targets.Clear();
		targets.AddRange(pawn.Map.mapPawns.AllPawnsSpawned);
		targets.SortBy((Pawn x) => x.Position.DistanceToSquared(pawn.Position));
		for (int num = targets.Count - 1; num >= 0; num--)
		{
			if (targets[num].DeadOrDowned || !pawn.HostileTo(targets[num]))
			{
				targets.RemoveAt(num);
			}
			else if (pawn.Position.InHorDistOf(targets[num].Position, 6f))
			{
				targets.RemoveAt(num);
			}
		}
		IntVec3 result;
		if (targets.Empty() || !TryGetIdealTarget(out var target))
		{
			if (!TryGetRandomPos(pawn, out result))
			{
				targets.Clear();
				return null;
			}
		}
		else
		{
			result = target.Position;
			if (pawn.Position.InHorDistOf(target.Position, 19.9f) && CellFinder.TryFindRandomCellNear(target.Position, pawn.Map, Mathf.RoundToInt(19.9f), (IntVec3 c) => IsValidTeleportCell(pawn, c) && GenSight.LineOfSightToThing(c, target, pawn.Map), out result))
			{
				return ability.GetJob(pawn, result);
			}
			IntVec3 intVec = result - pawn.Position;
			result = pawn.Position + intVec.ClampMagnitude(19.9f);
			float teleportRadius = GetTeleportRadius(pawn.Position.DistanceTo(result), 30f);
			if (!CellFinder.TryFindRandomCellNear(result, pawn.Map, Mathf.RoundToInt(teleportRadius), (IntVec3 c) => IsValidTeleportCell(pawn, c), out result) && !TryGetRandomPos(pawn, out result))
			{
				targets.Clear();
				return null;
			}
		}
		targets.Clear();
		if (!IsValidTeleportCell(pawn, result))
		{
			return null;
		}
		return ability.GetJob(pawn, result);
	}

	private bool TryGetIdealTarget(out Pawn target)
	{
		target = GetIdealTarget();
		return target != null;
	}

	private Pawn GetIdealTarget()
	{
		for (int i = 0; i < targets.Count; i++)
		{
			Pawn pawn = targets[i];
			if (!pawn.DeadOrDowned && (pawn.Faction != null || (int)pawn.RaceProps.intelligence >= 1))
			{
				return targets[i];
			}
		}
		return null;
	}

	private bool TryGetRandomPos(Pawn pawn, out IntVec3 cell)
	{
		if (CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 14, (IntVec3 c) => IsValidTeleportCell(pawn, c), out cell))
		{
			return true;
		}
		cell = IntVec3.Invalid;
		return false;
	}

	private static bool IsValidTeleportCell(Pawn pawn, IntVec3 cell)
	{
		if (!cell.Fogged(pawn.Map) && cell.Walkable(pawn.Map) && cell.InBounds(pawn.Map, 2) && pawn.Position.DistanceTo(cell) <= 19.9f)
		{
			return AreaClearOfPawns(pawn, cell);
		}
		return false;
	}

	private static bool AreaClearOfPawns(Pawn pawn, IntVec3 cell)
	{
		int num = GenRadial.NumCellsInRadius(3f);
		for (int i = 0; i < num; i++)
		{
			IntVec3 c = cell + GenRadial.RadialPattern[i];
			if (c.InBounds(pawn.Map) && c.GetFirstPawn(pawn.Map) != null)
			{
				return false;
			}
		}
		return true;
	}

	private static float GetTeleportRadius(float distance, float angleDegrees)
	{
		return distance * Mathf.Tan(angleDegrees / 2f * (MathF.PI / 180f));
	}
}
