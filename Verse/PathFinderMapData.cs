using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LudeonTK;
using RimWorld;
using Unity.Collections;
using Verse.AI;
using Verse.AI.Group;

namespace Verse;

public class PathFinderMapData : IDisposable
{
	private readonly Map map;

	private readonly CostSource normalCost;

	private readonly CostSource fenceBlockedCost;

	private readonly CostSource flyingCost;

	private readonly AreaSource areas;

	private readonly PerceptualSource perceptual;

	private readonly ConnectivitySource connectivity;

	private readonly WaterSource water;

	private readonly FenceSource fences;

	private readonly BuildingSource buildings;

	private readonly FactionSource factions;

	private readonly FogSource fogged;

	private readonly PersistentDangerSource persistentDanger;

	private readonly DarknessSource darknessDanger;

	private readonly List<IPathFinderDataSource> sources;

	private int lastGatherTick = -1;

	private readonly List<IntVec3> cellDeltas = new List<IntVec3>();

	private readonly HashSet<IntVec3> cellDeltaSet = new HashSet<IntVec3>();

	private readonly List<CellRect> cellRectDeltas = new List<CellRect>();

	private NativeArray<byte> emptyByteGrid = NativeArrayUtility.EmptyArray<byte>();

	private NativeArray<bool> emptyBoolGrid = NativeArrayUtility.EmptyArray<bool>();

	private NativeArray<ushort> emptyUShortGrid = NativeArrayUtility.EmptyArray<ushort>();

	private NativeBitArray emptyBitGrid = NativeArrayUtility.EmptyBitArray();

	public PathFinderMapData(Map map)
	{
		this.map = map;
		cellDeltas.Capacity = map.cellIndices.NumGridCells;
		cellDeltaSet.EnsureCapacity(map.cellIndices.NumGridCells);
		normalCost = new CostSource(map, map.pathing.Normal);
		fenceBlockedCost = new CostSource(map, map.pathing.FenceBlocked);
		flyingCost = new CostSource(map, map.pathing.Flying);
		areas = new AreaSource(map);
		perceptual = new PerceptualSource(map);
		connectivity = new ConnectivitySource(map);
		water = new WaterSource(map);
		fences = new FenceSource(map);
		buildings = new BuildingSource(map);
		factions = new FactionSource(map);
		fogged = new FogSource(map);
		persistentDanger = new PersistentDangerSource(map);
		darknessDanger = new DarknessSource(map);
		sources = new List<IPathFinderDataSource>
		{
			normalCost, fenceBlockedCost, flyingCost, areas, perceptual, connectivity, water, fences, buildings, factions,
			fogged, persistentDanger, darknessDanger
		};
		map.events.BuildingSpawned += Notify_BuildingChanged;
		map.events.BuildingDespawned += Notify_BuildingChanged;
		map.events.BuildingHitPointsChanged += Notify_BuildingChanged;
		map.events.ThingSpawned += Notify_ThingSpawnedDespawned;
		map.events.ThingDespawned += Notify_ThingSpawnedDespawned;
		map.events.ReservationAdded += Notify_Reservation;
		map.events.ReservationRemoved += Notify_Reservation;
		map.events.HaulEnrouteAdded += Notify_HaulEnroute;
		map.events.HaulEnrouteReleased += Notify_HaulReleased;
		map.events.FactionRemoved += Notify_FactionRemoved;
		map.events.TerrainChanged += Notify_CellDelta;
		map.events.PathCostRecalculate += Notify_CellDelta;
		map.events.CellFogChanged += Event_FogChanged;
		map.events.MapFogged += Notify_MapDirtied;
	}

	private void Notify_HaulEnroute(Thing enroute, Pawn pawn, ThingDef stuff, int count)
	{
		Notify_CellDelta(enroute.OccupiedRect());
	}

	private void Notify_HaulReleased(Thing enroute, Pawn pawn)
	{
		Notify_CellDelta(enroute.OccupiedRect());
	}

	private void Notify_ThingSpawnedDespawned(Thing thing)
	{
		if (thing.def.pathfinderDangerous && (!(thing is AttachableThing attachableThing) || !(attachableThing.parent is Pawn)))
		{
			if (thing.def.size.Area == 1)
			{
				Notify_CellDelta(thing.Position);
			}
			else
			{
				Notify_CellDelta(thing.OccupiedRect());
			}
		}
	}

	public void Dispose()
	{
		foreach (IPathFinderDataSource source in sources)
		{
			source.Dispose();
		}
		emptyByteGrid.Dispose();
		emptyBoolGrid.Dispose();
		emptyUShortGrid.Dispose();
		emptyBitGrid.Dispose();
	}

	public CellConnection CellConnectionsAt(int index)
	{
		return connectivity.Data[index];
	}

	private void Notify_Reservation(ReservationManager.Reservation reservation)
	{
		if (reservation.Target.HasThing)
		{
			Notify_CellDelta(reservation.Target.Thing.OccupiedRect());
		}
	}

	public void Notify_CellDelta(IntVec3 cell)
	{
		if (cell.InBounds(map))
		{
			if (cellDeltaSet.Add(cell))
			{
				cellDeltas.Add(cell);
			}
		}
		else
		{
			Log.Warning($"{this} was notified using an out-of-bounds cell: {cell}");
		}
	}

	private void Notify_MapDirtied()
	{
		lastGatherTick = -1;
	}

	private void Notify_BuildingChanged(Building building)
	{
		Notify_CellDelta(building.OccupiedRect());
	}

	private void Event_FogChanged(IntVec3 cell, bool _)
	{
		Notify_CellDelta(cell);
	}

	public void Notify_CellDelta(CellRect rect)
	{
		cellRectDeltas.Add(rect);
	}

	public void Notify_AreaDelta(Area area, IntVec3 cell)
	{
		areas.Notify_AreaDelta(area, cell);
	}

	public void Notify_FactionRemoved(Faction faction)
	{
		factions.Notify_Removed(faction);
	}

	public bool GatherData(IEnumerable<PathRequest> requests)
	{
		using (ProfilerBlock.Scope("PathFinderMapData.GatherData"))
		{
			if (lastGatherTick == GenTicks.TicksGame)
			{
				return false;
			}
			foreach (CellRect cellRectDelta in cellRectDeltas)
			{
				foreach (IntVec3 cell in cellRectDelta.Cells)
				{
					if (cell.InBounds(map) && cellDeltaSet.Add(cell))
					{
						cellDeltas.Add(cell);
					}
				}
			}
			bool anyChanged = cellDeltas.Any();
			cellRectDeltas.Clear();
			if (lastGatherTick >= 0)
			{
				using (ProfilerBlock.Scope("incremental"))
				{
					Parallel.ForEach(sources, delegate(IPathFinderDataSource source)
					{
						if (source.UpdateIncrementally(requests, cellDeltas))
						{
							anyChanged = true;
						}
					});
				}
			}
			else
			{
				anyChanged = true;
				using (ProfilerBlock.Scope("recompute"))
				{
					Parallel.ForEach(sources, delegate(IPathFinderDataSource source)
					{
						source.ComputeAll(requests);
					});
				}
			}
			cellDeltas.Clear();
			cellDeltaSet.Clear();
			lastGatherTick = GenTicks.TicksGame;
			return anyChanged;
		}
	}

	public void ParameterizePathJob(ref PathFinderJob job)
	{
		job.connectivity = connectivity.Data;
		job.fences = fences.Data;
		job.buildings = buildings.Buildings;
	}

	public void ParameterizeGridJob(PathRequest request, ref PathFinder.MapGridRequest query, ref PathGridJob job, ref PathFinder.GridJobOutput output)
	{
		Faction requesterFaction = request.RequesterFaction;
		Lord requesterLord = request.RequesterLord;
		job.traverseParams = query.traverseParams;
		job.tuning = query.tuning;
		job.indicies = map.cellIndices;
		job.grid = output.grid;
		job.pathGridDirect = (request.TraverseParms.fenceBlocked ? fenceBlockedCost.Data : normalCost.Data);
		job.building = buildings.Buildings;
		job.buildingDestroyable = buildings.Destroyable;
		job.player = buildings.Player;
		job.water = water.Data;
		job.fence = fences.Data;
		job.factionCosts = factions[requesterFaction];
		job.buildingHitPoints = buildings.Hitpoints;
		job.fogged = fogged.Data;
		job.darknessDanger = darknessDanger.Data;
		job.persistentDanger = persistentDanger.Data;
		Pawn pawn = request.pawn;
		if (pawn != null && pawn.Drafted)
		{
			job.perceptualCost = perceptual.CostDrafted;
		}
		else
		{
			job.perceptualCost = perceptual.CostUndrafted;
		}
		if (requesterLord != null)
		{
			NativeBitArray walkGrid = requesterLord.LordJob.GetWalkGrid(request.pawn);
			job.lordGrid = (walkGrid.IsCreated ? walkGrid.AsReadOnly() : emptyBitGrid.AsReadOnly());
		}
		else
		{
			job.lordGrid = emptyBitGrid.AsReadOnly();
		}
		job.allowedGrid = ((request.area != null) ? areas.DataForArea(request.area).AsReadOnly() : emptyBitGrid.AsReadOnly());
		job.custom = ((request.customizer != null) ? request.customizer.GetOffsetGrid().AsReadOnly() : emptyUShortGrid.AsReadOnly());
		job.avoidGrid = query.avoidGrid?.Grid ?? emptyByteGrid.AsReadOnly();
	}

	public void LogCell(IntVec3 cell)
	{
		int num = map.cellIndices.CellToIndex(cell);
		string text = $"Cell: {cell}\n" + $"Cost: {normalCost.Data[num]} (fenced: {fenceBlockedCost.Data[num]}, flying: {flyingCost.Data[num]})\n" + $"Connections: {CellConnectionsAt(num)}\n" + $"Building: {buildings.Buildings.IsSet(num)}\n" + $"Player: {buildings.Player.IsSet(num)}\n" + $"Fence: {fences.Data.IsSet(num)}";
		foreach (Faction allFaction in Find.FactionManager.AllFactions)
		{
			if (factions.HasFactionData(allFaction))
			{
				text += $"\n    [{allFaction.def.defName}]: {factions[allFaction][num]}";
			}
		}
		Log.Message(text);
	}

	public void RegisterSource(IPathFinderDataSource source)
	{
		if (sources.Contains(source))
		{
			throw new InvalidOperationException($"Source {source} is already registered in {this}");
		}
		sources.Add(source);
	}
}
