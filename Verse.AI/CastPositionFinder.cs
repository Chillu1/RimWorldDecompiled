using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Unity.Collections;
using UnityEngine;

namespace Verse.AI;

public static class CastPositionFinder
{
	private static CastPositionRequest req;

	private static IntVec3 casterLoc;

	private static IntVec3 targetLoc;

	private static Verb verb;

	private static float rangeFromTarget;

	private static float rangeFromTargetSquared;

	private static float optimalRangeSquared;

	private static float rangeFromCasterToCellSquared;

	private static float rangeFromTargetToCellSquared;

	private static int inRadiusMark;

	private static NativeArray<byte>.ReadOnly avoidGrid;

	private static float maxRangeFromCasterSquared;

	private static float maxRangeFromTargetSquared;

	private static float maxRangeFromLocusSquared;

	private static IntVec3 bestSpot;

	private static float bestSpotPref;

	private static NativeArray<byte> emptyByteArray;

	private const float BaseAIPreference = 0.3f;

	private const float MinimumPreferredRange = 5f;

	private const float OptimalRangeFactor = 0.8f;

	private const float OptimalRangeFactorImportance = 0.3f;

	private const float CoverPreferenceFactor = 0.55f;

	static CastPositionFinder()
	{
		bestSpot = IntVec3.Invalid;
		bestSpotPref = 0.001f;
		emptyByteArray = NativeArrayUtility.EmptyArray<byte>();
		UnityData.DisposeStatic += delegate
		{
			emptyByteArray.Dispose();
		};
	}

	public static bool TryFindCastPosition(CastPositionRequest newReq, out IntVec3 dest)
	{
		req = newReq;
		casterLoc = req.caster.Position;
		targetLoc = req.target.Position;
		verb = req.verb;
		avoidGrid = (newReq.caster.TryGetAvoidGrid(out var grid, onlyIfLordAllows: false) ? grid.Grid : emptyByteArray.AsReadOnly());
		if (verb == null)
		{
			Log.Error(req.caster?.ToString() + " tried to find casting position without a verb.");
			dest = IntVec3.Invalid;
			req = default(CastPositionRequest);
			return false;
		}
		if (req.maxRegions > 0)
		{
			Region region = casterLoc.GetRegion(req.caster.Map);
			if (region == null)
			{
				Log.Error("TryFindCastPosition requiring region traversal but root region is null.");
				dest = IntVec3.Invalid;
				req = default(CastPositionRequest);
				verb = null;
				return false;
			}
			inRadiusMark = Rand.Int;
			RegionTraverser.MarkRegionsBFS(region, null, newReq.maxRegions, inRadiusMark);
			if (req.maxRangeFromLocus > 0.01f)
			{
				Region locusReg = req.locus.GetRegion(req.caster.Map);
				if (locusReg == null)
				{
					IntVec3 locus = req.locus;
					Log.Error("locus " + locus.ToString() + " has no region");
					dest = IntVec3.Invalid;
					req = default(CastPositionRequest);
					verb = null;
					return false;
				}
				if (locusReg.mark != inRadiusMark)
				{
					inRadiusMark = Rand.Int;
					RegionTraverser.BreadthFirstTraverse(region, null, delegate(Region r)
					{
						r.mark = inRadiusMark;
						req.maxRegions++;
						return r == locusReg;
					});
				}
			}
		}
		CellRect cellRect = CellRect.WholeMap(req.caster.Map);
		if (req.maxRangeFromCaster > 0.01f)
		{
			int num = Mathf.CeilToInt(req.maxRangeFromCaster);
			CellRect otherRect = new CellRect(casterLoc.x - num, casterLoc.z - num, num * 2 + 1, num * 2 + 1);
			cellRect.ClipInsideRect(otherRect);
		}
		int num2 = Mathf.CeilToInt(req.maxRangeFromTarget);
		CellRect otherRect2 = new CellRect(targetLoc.x - num2, targetLoc.z - num2, num2 * 2 + 1, num2 * 2 + 1);
		cellRect.ClipInsideRect(otherRect2);
		if (req.maxRangeFromLocus > 0.01f)
		{
			int num3 = Mathf.CeilToInt(req.maxRangeFromLocus);
			CellRect otherRect3 = new CellRect(targetLoc.x - num3, targetLoc.z - num3, num3 * 2 + 1, num3 * 2 + 1);
			cellRect.ClipInsideRect(otherRect3);
		}
		bestSpot = IntVec3.Invalid;
		bestSpotPref = 0.001f;
		maxRangeFromCasterSquared = req.maxRangeFromCaster * req.maxRangeFromCaster;
		maxRangeFromTargetSquared = req.maxRangeFromTarget * req.maxRangeFromTarget;
		maxRangeFromLocusSquared = req.maxRangeFromLocus * req.maxRangeFromLocus;
		rangeFromTarget = (req.caster.Position - req.target.Position).LengthHorizontal;
		rangeFromTargetSquared = (req.caster.Position - req.target.Position).LengthHorizontalSquared;
		optimalRangeSquared = verb.EffectiveRange * 0.8f * (verb.verbProps.range * 0.8f);
		if (req.preferredCastPosition.HasValue && req.preferredCastPosition.Value.IsValid)
		{
			EvaluateCell(req.preferredCastPosition.Value);
			if (bestSpot.IsValid && bestSpotPref > 0.001f)
			{
				dest = req.preferredCastPosition.Value;
				req = default(CastPositionRequest);
				verb = null;
				return true;
			}
		}
		EvaluateCell(req.caster.Position);
		if ((double)bestSpotPref >= 1.0)
		{
			dest = req.caster.Position;
			req = default(CastPositionRequest);
			verb = null;
			return true;
		}
		float slope = -1f / CellLine.Between(req.target.Position, req.caster.Position).Slope;
		CellLine cellLine = new CellLine(req.target.Position, slope);
		bool flag = cellLine.CellIsAbove(req.caster.Position);
		foreach (IntVec3 item in cellRect)
		{
			if (cellLine.CellIsAbove(item) == flag && cellRect.Contains(item))
			{
				EvaluateCell(item);
			}
		}
		if (bestSpot.IsValid && bestSpotPref > 0.33f)
		{
			dest = bestSpot;
			req = default(CastPositionRequest);
			verb = null;
			return true;
		}
		foreach (IntVec3 item2 in cellRect)
		{
			if (cellLine.CellIsAbove(item2) != flag && cellRect.Contains(item2))
			{
				EvaluateCell(item2);
			}
		}
		if (bestSpot.IsValid)
		{
			dest = bestSpot;
			req = default(CastPositionRequest);
			verb = null;
			return true;
		}
		dest = casterLoc;
		req = default(CastPositionRequest);
		verb = null;
		return false;
	}

	private static void EvaluateCell(IntVec3 c)
	{
		if (req.validator != null && !req.validator(c))
		{
			return;
		}
		if (maxRangeFromTargetSquared > 0.01f && maxRangeFromTargetSquared < 250000f && (float)(c - req.target.Position).LengthHorizontalSquared > maxRangeFromTargetSquared)
		{
			if (DebugViewSettings.drawCastPositionSearch)
			{
				req.caster.Map.debugDrawer.FlashCell(c, 0f, "range target");
			}
			return;
		}
		if ((double)maxRangeFromLocusSquared > 0.01 && (float)(c - req.locus).LengthHorizontalSquared > maxRangeFromLocusSquared)
		{
			if (DebugViewSettings.drawCastPositionSearch)
			{
				req.caster.Map.debugDrawer.FlashCell(c, 0.1f, "range home");
			}
			return;
		}
		if (maxRangeFromCasterSquared > 0.01f)
		{
			rangeFromCasterToCellSquared = (c - req.caster.Position).LengthHorizontalSquared;
			if (rangeFromCasterToCellSquared > maxRangeFromCasterSquared)
			{
				if (DebugViewSettings.drawCastPositionSearch)
				{
					req.caster.Map.debugDrawer.FlashCell(c, 0.2f, "range caster");
				}
				return;
			}
		}
		if (!c.WalkableBy(req.caster.Map, req.caster) || (req.caster.Position != c && !c.InAllowedArea(req.caster)))
		{
			return;
		}
		if (req.maxRegions > 0 && c.GetRegion(req.caster.Map).mark != inRadiusMark)
		{
			if (DebugViewSettings.drawCastPositionSearch)
			{
				req.caster.Map.debugDrawer.FlashCell(c, 0.64f, "reg radius");
			}
			return;
		}
		if (!req.caster.Map.reachability.CanReach(req.caster.Position, c, PathEndMode.OnCell, TraverseParms.For(req.caster, Danger.Some)))
		{
			if (DebugViewSettings.drawCastPositionSearch)
			{
				req.caster.Map.debugDrawer.FlashCell(c, 0.4f, "can't reach");
			}
			return;
		}
		float num = CastPositionPreference(c);
		if (avoidGrid.Length > 0)
		{
			byte b = avoidGrid[req.caster.Map.cellIndices.CellToIndex(c)];
			num *= Mathf.Max(0.1f, (37.5f - (float)(int)b) / 37.5f);
		}
		if (DebugViewSettings.drawCastPositionSearch)
		{
			req.caster.Map.debugDrawer.FlashCell(c, num / 4f, num.ToString("F3"));
		}
		if (num < bestSpotPref)
		{
			return;
		}
		if (!verb.CanHitTargetFrom(c, req.target))
		{
			if (DebugViewSettings.drawCastPositionSearch)
			{
				req.caster.Map.debugDrawer.FlashCell(c, 0.6f, "can't hit");
			}
		}
		else if (!req.caster.Map.pawnDestinationReservationManager.CanReserve(c, req.caster))
		{
			if (DebugViewSettings.drawCastPositionSearch)
			{
				req.caster.Map.debugDrawer.FlashCell(c, num * 0.9f, "resvd");
			}
		}
		else if (PawnUtility.KnownDangerAt(c, req.caster.Map, req.caster))
		{
			if (DebugViewSettings.drawCastPositionSearch)
			{
				req.caster.Map.debugDrawer.FlashCell(c, 0.9f, "danger");
			}
		}
		else
		{
			bestSpot = c;
			bestSpotPref = num;
		}
	}

	private static float CastPositionPreference(IntVec3 c)
	{
		bool flag = true;
		List<Thing> list = req.caster.Map.thingGrid.ThingsListAtFast(c);
		for (int i = 0; i < list.Count; i++)
		{
			Thing thing = list[i];
			if (thing is Fire { parent: null })
			{
				return -1f;
			}
			if (thing.def.passability == Traversability.PassThroughOnly)
			{
				flag = false;
			}
		}
		float num = 0.3f;
		if (req.caster.kindDef.aiAvoidCover)
		{
			num += 8f - CoverUtility.TotalSurroundingCoverScore(c, req.caster.Map);
		}
		if (req.wantCoverFromTarget)
		{
			num += CoverUtility.CalculateOverallBlockChance(c, req.target.Position, req.caster.Map) * 0.55f;
		}
		float num2 = (req.caster.Position - c).LengthHorizontal;
		if (rangeFromTarget > 100f)
		{
			num2 -= rangeFromTarget - 100f;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
		}
		num *= Mathf.Pow(0.967f, num2);
		float num3 = 1f;
		rangeFromTargetToCellSquared = (c - req.target.Position).LengthHorizontalSquared;
		float num4 = Mathf.Abs(rangeFromTargetToCellSquared - optimalRangeSquared) / optimalRangeSquared;
		num4 = 1f - num4;
		num4 = 0.7f + 0.3f * num4;
		num3 *= num4;
		if (rangeFromTargetToCellSquared < 25f)
		{
			num3 *= 0.5f;
		}
		num *= num3;
		if (rangeFromCasterToCellSquared > rangeFromTargetSquared)
		{
			num *= 0.4f;
		}
		if (!flag)
		{
			num *= 0.4f;
		}
		return num;
	}
}
