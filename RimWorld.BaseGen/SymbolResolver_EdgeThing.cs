using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_EdgeThing : SymbolResolver
{
	private List<int> randomRotations = new List<int> { 0, 1, 2, 3 };

	private int MaxTriesToAvoidOtherEdgeThings = 4;

	public override bool CanResolve(ResolveParams rp)
	{
		if (!base.CanResolve(rp))
		{
			return false;
		}
		if (rp.singleThingDef != null)
		{
			bool valueOrDefault = rp.edgeThingAvoidOtherEdgeThings == true;
			bool valueOrDefault2 = rp.edgeThingMustReachMapEdge == true;
			IntVec3 spawnCell;
			if (rp.thingRot.HasValue)
			{
				if (!TryFindSpawnCell(rp.rect, rp.singleThingDef, rp.thingRot.Value, valueOrDefault, valueOrDefault2, out spawnCell))
				{
					return false;
				}
			}
			else if (!rp.singleThingDef.rotatable)
			{
				if (!TryFindSpawnCell(rp.rect, rp.singleThingDef, Rot4.North, valueOrDefault, valueOrDefault2, out spawnCell))
				{
					return false;
				}
			}
			else
			{
				bool flag = false;
				for (int i = 0; i < 4; i++)
				{
					if (TryFindSpawnCell(rp.rect, rp.singleThingDef, new Rot4(i), valueOrDefault, valueOrDefault2, out spawnCell))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void Resolve(ResolveParams rp)
	{
		ThingDef thingDef = rp.singleThingDef ?? DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => (x.IsWeapon || x.IsMedicine || x.IsDrug) && x.graphicData != null && !x.destroyOnDrop && x.size.x <= rp.rect.Width && x.size.z <= rp.rect.Width && x.size.x <= rp.rect.Height && x.size.z <= rp.rect.Height).RandomElement();
		IntVec3 spawnCell = IntVec3.Invalid;
		Rot4 value = Rot4.North;
		bool valueOrDefault = rp.edgeThingAvoidOtherEdgeThings == true;
		bool valueOrDefault2 = rp.edgeThingMustReachMapEdge == true;
		if (rp.thingRot.HasValue)
		{
			if (!TryFindSpawnCell(rp.rect, thingDef, rp.thingRot.Value, valueOrDefault, valueOrDefault2, out spawnCell))
			{
				return;
			}
			value = rp.thingRot.Value;
		}
		else if (!thingDef.rotatable)
		{
			if (!TryFindSpawnCell(rp.rect, thingDef, Rot4.North, valueOrDefault, valueOrDefault2, out spawnCell))
			{
				return;
			}
			value = Rot4.North;
		}
		else
		{
			randomRotations.Shuffle();
			bool flag = false;
			for (int num = 0; num < randomRotations.Count; num++)
			{
				if (TryFindSpawnCell(rp.rect, thingDef, new Rot4(randomRotations[num]), valueOrDefault, valueOrDefault2, out spawnCell))
				{
					value = new Rot4(randomRotations[num]);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
		}
		ResolveParams resolveParams = rp;
		resolveParams.rect = CellRect.SingleCell(spawnCell);
		resolveParams.thingRot = value;
		resolveParams.singleThingDef = thingDef;
		BaseGen.symbolStack.Push("thing", resolveParams);
	}

	private bool TryFindSpawnCell(CellRect rect, ThingDef thingDef, Rot4 rot, bool avoidOtherEdgeThings, bool mustReachMapEdge, out IntVec3 spawnCell)
	{
		if (avoidOtherEdgeThings)
		{
			spawnCell = IntVec3.Invalid;
			int num = -1;
			for (int i = 0; i < MaxTriesToAvoidOtherEdgeThings; i++)
			{
				if (!TryFindSpawnCell(rect, thingDef, rot, mustReachMapEdge, out var spawnCell2))
				{
					continue;
				}
				int distanceSquaredToExistingEdgeThing = GetDistanceSquaredToExistingEdgeThing(spawnCell2, rect, thingDef);
				if (!spawnCell.IsValid || distanceSquaredToExistingEdgeThing > num)
				{
					spawnCell = spawnCell2;
					num = distanceSquaredToExistingEdgeThing;
					if (num == int.MaxValue)
					{
						break;
					}
				}
			}
			return spawnCell.IsValid;
		}
		return TryFindSpawnCell(rect, thingDef, rot, mustReachMapEdge, out spawnCell);
	}

	private bool TryFindSpawnCell(CellRect rect, ThingDef thingDef, Rot4 rot, bool mustReachMapEdge, out IntVec3 spawnCell)
	{
		Map map = BaseGen.globalSettings.map;
		IntVec3 center = IntVec3.Zero;
		IntVec2 size = thingDef.size;
		GenAdj.AdjustForRotation(ref center, ref size, rot);
		CellRect rect2 = CellRect.Empty;
		TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
		Predicate<CellRect> basePredicate = (CellRect x) => x.Cells.All((IntVec3 y) => y.Standable(map)) && !GenSpawn.WouldWipeAnythingWith(x, thingDef, map, (Thing z) => z.def.category == ThingCategory.Building) && (thingDef.category != ThingCategory.Item || x.CenterCell.GetFirstItem(map) == null) && (!mustReachMapEdge || map.reachability.CanReachMapEdge(x.CenterCell, traverseParms));
		bool flag = false;
		if (thingDef.category == ThingCategory.Building)
		{
			flag = rect.TryFindRandomInnerRectTouchingEdge(size, out rect2, (CellRect x) => basePredicate(x) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(x, map) && GenConstruct.TerrainCanSupport(x, map, thingDef));
			if (!flag)
			{
				flag = rect.TryFindRandomInnerRectTouchingEdge(size, out rect2, (CellRect x) => basePredicate(x) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(x, map));
			}
		}
		if (!flag && !rect.TryFindRandomInnerRectTouchingEdge(size, out rect2, basePredicate))
		{
			spawnCell = IntVec3.Invalid;
			return false;
		}
		foreach (IntVec3 item in rect2)
		{
			if (GenAdj.OccupiedRect(item, rot, thingDef.size) == rect2)
			{
				spawnCell = item;
				return true;
			}
		}
		Log.Error("We found a valid rect but we couldn't find the root position. This should never happen.");
		spawnCell = IntVec3.Invalid;
		return false;
	}

	private int GetDistanceSquaredToExistingEdgeThing(IntVec3 cell, CellRect rect, ThingDef thingDef)
	{
		Map map = BaseGen.globalSettings.map;
		int num = int.MaxValue;
		foreach (IntVec3 edgeCell in rect.EdgeCells)
		{
			List<Thing> thingList = edgeCell.GetThingList(map);
			bool flag = false;
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def == thingDef)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				num = Mathf.Min(num, cell.DistanceToSquared(edgeCell));
			}
		}
		return num;
	}
}
