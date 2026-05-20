using System;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;

namespace Verse;

public class MapEvents
{
	public readonly Map map;

	public event Action<Building> BuildingSpawned;

	public event Action<Building> BuildingDespawned;

	public event Action<Thing> ThingSpawned;

	public event Action<Thing> ThingDespawned;

	public event Action<GameCondition> GameConditionAdded;

	public event Action<GameCondition> GameConditionRemoved;

	public event Action<Lord> LordAdded;

	public event Action<Lord> LordRemoved;

	public event Action<Faction> FactionAdded;

	public event Action<Faction> FactionRemoved;

	public event Action<Faction, Faction> ThingFactionChanged;

	public event Action<Building> BuildingHitPointsChanged;

	public event Action<IntVec3> TerrainChanged;

	public event Action<IntVec3> PathCostRecalculate;

	public event Action<ReservationManager.Reservation> ReservationAdded;

	public event Action<ReservationManager.Reservation> ReservationRemoved;

	public event Action<Thing, Pawn, ThingDef, int> HaulEnrouteAdded;

	public event Action<Thing, Pawn> HaulEnrouteReleased;

	public event Action<IntVec3, bool> CellFogChanged;

	public event Action MapFogged;

	public event Action RegionsRoomsChanged;

	public event Action<Building_Door> DoorOpened;

	public event Action<Building_Door> DoorClosed;

	public event Action<IntVec3> RoofChanged;

	public event Action<IntVec3> GlowChanged;

	public MapEvents(Map map)
	{
		this.map = map;
	}

	public void Notify_BuildingSpawned(Building building)
	{
		this.BuildingSpawned?.Invoke(building);
	}

	public void Notify_BuildingDespawned(Building building)
	{
		this.BuildingDespawned?.Invoke(building);
	}

	public void Notify_ThingSpawned(Thing thing)
	{
		this.ThingSpawned?.Invoke(thing);
	}

	public void Notify_ThingDespawned(Thing thing)
	{
		this.ThingDespawned?.Invoke(thing);
	}

	public void Notify_GameConditionAdded(GameCondition condition)
	{
		this.GameConditionAdded?.Invoke(condition);
	}

	public void Notify_GameConditionRemoved(GameCondition condition)
	{
		this.GameConditionRemoved?.Invoke(condition);
	}

	public void Notify_LordAdded(Lord lord)
	{
		this.LordAdded?.Invoke(lord);
	}

	public void Notify_LordRemoved(Lord lord)
	{
		this.LordRemoved?.Invoke(lord);
	}

	public void Notify_FactionAdded(Faction faction)
	{
		this.FactionAdded?.Invoke(faction);
	}

	public void Notify_FactionRemoved(Faction faction)
	{
		this.FactionRemoved?.Invoke(faction);
	}

	public void Notify_ThingFactionChanged(Faction previous, Faction faction)
	{
		this.ThingFactionChanged?.Invoke(previous, faction);
	}

	public void Notify_BuildingHitPointsChanged(Building building)
	{
		this.BuildingHitPointsChanged?.Invoke(building);
	}

	public void Notify_TerrainChanged(IntVec3 cell)
	{
		this.TerrainChanged?.Invoke(cell);
	}

	public void Notify_PathCostRecalculated(IntVec3 cell)
	{
		this.PathCostRecalculate?.Invoke(cell);
	}

	public void Notify_ReservationAdded(ReservationManager.Reservation reservation)
	{
		this.ReservationAdded?.Invoke(reservation);
	}

	public void Notify_ReservationRemoved(ReservationManager.Reservation reservation)
	{
		this.ReservationRemoved?.Invoke(reservation);
	}

	public void Notify_HaulEnrouteAdded(Thing enroute, Pawn pawn, ThingDef stuff, int count)
	{
		this.HaulEnrouteAdded?.Invoke(enroute, pawn, stuff, count);
	}

	public void Notify_HaulEnrouteReleased(Thing enroute, Pawn pawn)
	{
		this.HaulEnrouteReleased?.Invoke(enroute, pawn);
	}

	public void Notify_CellFogChanged(IntVec3 cell, bool fogged)
	{
		this.CellFogChanged?.Invoke(cell, fogged);
	}

	public void Notify_MapFogged()
	{
		this.MapFogged?.Invoke();
	}

	public void Notify_RegionsRoomsChanged()
	{
		this.RegionsRoomsChanged?.Invoke();
	}

	public void Notify_DoorOpened(Building_Door door)
	{
		this.DoorOpened?.Invoke(door);
	}

	public void Notify_DoorClosed(Building_Door door)
	{
		this.DoorClosed?.Invoke(door);
	}

	public void Notify_RoofChanged(IntVec3 cell)
	{
		this.RoofChanged?.Invoke(cell);
	}

	public void Notify_GlowChanged(IntVec3 cell)
	{
		this.GlowChanged?.Invoke(cell);
	}
}
