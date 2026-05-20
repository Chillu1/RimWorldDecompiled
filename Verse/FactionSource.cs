using System;
using System.Collections.Generic;
using LudeonTK;
using RimWorld;
using Unity.Collections;

namespace Verse;

public class FactionSource : IPathFinderDataSource, IDisposable
{
	public const ushort AvoidUnderConstructionPathFindCost = 400;

	private readonly Map map;

	private readonly int cellCount;

	private NativeArray<ushort> noCost;

	private readonly Dictionary<Faction, NativeArray<ushort>> factionCosts = new Dictionary<Faction, NativeArray<ushort>>();

	private readonly List<Faction> tmpToUpdate = new List<Faction>();

	public NativeArray<ushort>.ReadOnly this[Faction faction]
	{
		get
		{
			if (faction != null && factionCosts.TryGetValue(faction, out var value))
			{
				return value.AsReadOnly();
			}
			return noCost.AsReadOnly();
		}
	}

	public FactionSource(Map map)
	{
		this.map = map;
		cellCount = map.cellIndices.NumGridCells;
		noCost = new NativeArray<ushort>(cellCount, Allocator.Persistent);
	}

	public void Dispose()
	{
		noCost.Dispose();
		foreach (NativeArray<ushort> value in factionCosts.Values)
		{
			value.Dispose();
		}
	}

	public bool HasFactionData(Faction faction)
	{
		if (faction == null)
		{
			return true;
		}
		return factionCosts.ContainsKey(faction);
	}

	public void Notify_Removed(Faction faction)
	{
		if (factionCosts.TryGetValue(faction, out var value))
		{
			value.Dispose();
			factionCosts.Remove(faction);
		}
	}

	public void ComputeAll(IEnumerable<PathRequest> requests)
	{
		GetFactionsToUpdate(requests);
		foreach (Faction item in tmpToUpdate)
		{
			ComputeAllForFaction(item);
		}
	}

	public bool UpdateIncrementally(IEnumerable<PathRequest> requests, List<IntVec3> cellDeltas)
	{
		GetFactionsToUpdate(requests);
		foreach (Faction item in tmpToUpdate)
		{
			if (!factionCosts.TryGetValue(item, out var value))
			{
				ComputeAllForFaction(item);
				continue;
			}
			CellIndices cellIndices = map.cellIndices;
			List<Blueprint>[] innerArray = map.blueprintGrid.InnerArray;
			Building[] innerArray2 = map.edificeGrid.InnerArray;
			foreach (IntVec3 cellDelta in cellDeltas)
			{
				int num = cellIndices.CellToIndex(cellDelta);
				ushort num2 = 0;
				List<Blueprint> list = innerArray[num];
				if (list != null)
				{
					foreach (Blueprint item2 in list)
					{
						num2 += PathFindCost(item2, item);
					}
				}
				if (innerArray2[num] is Frame thing)
				{
					num2 += PathFindCost(thing, item);
				}
				value[num] = num2;
			}
		}
		return false;
	}

	private void GetFactionsToUpdate(IEnumerable<PathRequest> requests)
	{
		tmpToUpdate.Clear();
		if (requests != null)
		{
			foreach (PathRequest request in requests)
			{
				Faction requesterFaction = request.RequesterFaction;
				if (requesterFaction != null)
				{
					tmpToUpdate.AddUnique(requesterFaction);
				}
			}
		}
		tmpToUpdate.AddRangeUnique(factionCosts.Keys);
	}

	private void ComputeAllForFaction(Faction faction)
	{
		if (!factionCosts.TryGetValue(faction, out var value))
		{
			value = new NativeArray<ushort>(cellCount, Allocator.Persistent);
			factionCosts[faction] = value;
		}
		else
		{
			value.Clear();
		}
		CellIndices cellIndices = map.cellIndices;
		IReadOnlyList<Thing> readOnlyList = map.listerThings.ThingsInGroup(ThingRequestGroup.Blueprint);
		IReadOnlyList<Thing> readOnlyList2 = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingFrame);
		int count = readOnlyList.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(readOnlyList[i] is Blueprint blueprint))
			{
				continue;
			}
			ushort num = PathFindCost(blueprint, faction);
			foreach (IntVec3 item in blueprint.OccupiedRect())
			{
				value[cellIndices.CellToIndex(item)] += num;
			}
		}
		int count2 = readOnlyList2.Count;
		for (int j = 0; j < count2; j++)
		{
			if (!(readOnlyList2[j] is Frame frame))
			{
				continue;
			}
			ushort num2 = PathFindCost(frame, faction);
			foreach (IntVec3 item2 in frame.OccupiedRect())
			{
				value[cellIndices.CellToIndex(item2)] += num2;
			}
		}
	}

	private ushort PathFindCost(Thing thing, Faction patherFaction)
	{
		Faction faction = thing.Faction;
		if (faction == null)
		{
			return 0;
		}
		if (thing.def.entityDefToBuild.passability == Traversability.Standable)
		{
			return 0;
		}
		if (!map.reservationManager.IsReservedAndRespected(thing, patherFaction))
		{
			return 0;
		}
		if (faction == patherFaction)
		{
			return 400;
		}
		if (faction.AllyOrNeutralTo(patherFaction))
		{
			return 400;
		}
		return 0;
	}
}
