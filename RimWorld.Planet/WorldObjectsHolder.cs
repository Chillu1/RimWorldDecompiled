using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class WorldObjectsHolder : IExposable
{
	private List<WorldObject> worldObjects = new List<WorldObject>();

	private readonly Dictionary<PlanetLayer, List<WorldObject>> layerWorldObjects = new Dictionary<PlanetLayer, List<WorldObject>>();

	private readonly HashSet<WorldObject> worldObjectsHashSet = new HashSet<WorldObject>();

	private readonly List<Caravan> caravans = new List<Caravan>();

	private readonly List<Settlement> settlements = new List<Settlement>();

	private readonly List<TravellingTransporters> travellingTransporters = new List<TravellingTransporters>();

	private readonly List<Settlement> settlementBases = new List<Settlement>();

	private readonly List<DestroyedSettlement> destroyedSettlements = new List<DestroyedSettlement>();

	private readonly List<RoutePlannerWaypoint> routePlannerWaypoints = new List<RoutePlannerWaypoint>();

	private readonly List<MapParent> mapParents = new List<MapParent>();

	private readonly List<Site> sites = new List<Site>();

	private readonly List<PeaceTalks> peaceTalks = new List<PeaceTalks>();

	private static readonly List<WorldObject> EmptyWorldObjectList = new List<WorldObject>();

	private static readonly List<WorldObject> tmpSettlements = new List<WorldObject>();

	private List<PlanetTile> tmpReservedTiles = new List<PlanetTile>();

	private List<ILoadReferenceable> tmpTileReservers = new List<ILoadReferenceable>();

	private static readonly List<WorldObject> tmpUnsavedWorldObjects = new List<WorldObject>();

	private static readonly List<WorldObject> tmpWorldObjects = new List<WorldObject>();

	public List<WorldObject> AllWorldObjects => worldObjects;

	public List<Caravan> Caravans => caravans;

	public List<Settlement> Settlements => settlements;

	public List<TravellingTransporters> TravellingTransporters => travellingTransporters;

	public List<Settlement> SettlementBases => settlementBases;

	public List<DestroyedSettlement> DestroyedSettlements => destroyedSettlements;

	public List<RoutePlannerWaypoint> RoutePlannerWaypoints => routePlannerWaypoints;

	public List<MapParent> MapParents => mapParents;

	public List<Site> Sites => sites;

	public List<PeaceTalks> PeaceTalks => peaceTalks;

	public int WorldObjectsCount => worldObjects.Count;

	public int CaravansCount => caravans.Count;

	public int RoutePlannerWaypointsCount => routePlannerWaypoints.Count;

	public int PlayerControlledCaravansCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < caravans.Count; i++)
			{
				if (caravans[i].IsPlayerControlled)
				{
					num++;
				}
			}
			return num;
		}
	}

	public List<WorldObject> AllWorldObjectsOnLayer(PlanetLayer layer)
	{
		return layerWorldObjects.GetValueOrDefault(layer, EmptyWorldObjectList);
	}

	public List<WorldObject> AllSettlementsOnLayer(PlanetLayer layer)
	{
		if (!layerWorldObjects.TryGetValue(layer, out var value))
		{
			return EmptyWorldObjectList;
		}
		tmpSettlements.Clear();
		foreach (WorldObject item in value)
		{
			if (item is Settlement)
			{
				tmpSettlements.Add(item);
			}
		}
		return tmpSettlements;
	}

	public bool AnyFactionSettlementOnLayer(Faction faction, PlanetLayer layer)
	{
		if (!layerWorldObjects.TryGetValue(layer, out var value))
		{
			return false;
		}
		foreach (WorldObject item in value)
		{
			if (item is Settlement settlement && settlement.Faction == faction)
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyFactionSettlementOnRootSurface(Faction faction)
	{
		return AnyFactionSettlementOnLayer(faction, Find.WorldGrid.Surface);
	}

	public void ExposeData()
	{
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			tmpUnsavedWorldObjects.Clear();
			for (int num = worldObjects.Count - 1; num >= 0; num--)
			{
				if (!worldObjects[num].def.saved)
				{
					tmpUnsavedWorldObjects.Add(worldObjects[num]);
					worldObjects.RemoveAt(num);
				}
			}
		}
		Scribe_Collections.Look(ref worldObjects, "worldObjects", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.Saving)
		{
			worldObjects.AddRange(tmpUnsavedWorldObjects);
			tmpUnsavedWorldObjects.Clear();
		}
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			worldObjects.RemoveAll((WorldObject wo) => wo == null);
			Recache();
		}
		if (Scribe.mode != LoadSaveMode.PostLoadInit)
		{
			return;
		}
		if (worldObjects.RemoveAll((WorldObject wo) => wo == null || wo.def == null) != 0)
		{
			Log.Error("Some WorldObjects had null def after loading.");
		}
		for (int num2 = worldObjects.Count - 1; num2 >= 0; num2--)
		{
			try
			{
				worldObjects[num2].SpawnSetup();
			}
			catch (Exception arg)
			{
				Log.Error($"Exception spawning WorldObject: {arg}");
				worldObjects.RemoveAt(num2);
			}
		}
	}

	public void Add(WorldObject o)
	{
		if (worldObjects.Contains(o))
		{
			Log.Error($"Tried to add world object {o} to world, but it's already here.");
			return;
		}
		if (!o.Tile.Valid && !(o is PocketMapParent))
		{
			Log.Error($"Tried to add world object {o} but its tile is not set. Setting to 0.");
			o.Tile = new PlanetTile(0);
		}
		worldObjects.Add(o);
		AddToCache(o);
		o.SpawnSetup();
		o.PostAdd();
	}

	public void Remove(WorldObject o)
	{
		if (!worldObjects.Contains(o))
		{
			Log.Error($"Tried to remove world object {o} from world, but it's not here.");
			return;
		}
		worldObjects.Remove(o);
		RemoveFromCache(o);
		o.PostRemove();
	}

	public void WorldObjectsHolderTick()
	{
		tmpWorldObjects.Clear();
		tmpWorldObjects.AddRange(worldObjects);
		for (int i = 0; i < tmpWorldObjects.Count; i++)
		{
			tmpWorldObjects[i].DoTick();
		}
	}

	private void AddToCache(WorldObject o)
	{
		worldObjectsHashSet.Add(o);
		if (o.Tile.Valid)
		{
			if (!layerWorldObjects.TryGetValue(o.Tile.Layer, out var value))
			{
				List<WorldObject> list = (layerWorldObjects[o.Tile.Layer] = new List<WorldObject>());
				value = list;
			}
			value.Add(o);
		}
		if (o is Caravan item)
		{
			caravans.Add(item);
		}
		if (o is Settlement item2)
		{
			settlements.Add(item2);
			settlementBases.Add(item2);
		}
		if (o is TravellingTransporters item3)
		{
			travellingTransporters.Add(item3);
		}
		if (o is DestroyedSettlement item4)
		{
			destroyedSettlements.Add(item4);
		}
		if (o is RoutePlannerWaypoint item5)
		{
			routePlannerWaypoints.Add(item5);
		}
		if (o is MapParent item6)
		{
			mapParents.Add(item6);
		}
		if (o is Site item7)
		{
			sites.Add(item7);
		}
		if (o is PeaceTalks item8)
		{
			peaceTalks.Add(item8);
		}
		ExpandableLandmarksUtility.Notify_WorldObjectsChanged();
	}

	private void RemoveFromCache(WorldObject o)
	{
		worldObjectsHashSet.Remove(o);
		if (o.Tile.Valid && layerWorldObjects.TryGetValue(o.Tile.Layer, out var value))
		{
			value.Remove(o);
		}
		if (o is Caravan item)
		{
			caravans.Remove(item);
		}
		if (o is Settlement item2)
		{
			settlements.Remove(item2);
			settlementBases.Remove(item2);
		}
		if (o is TravellingTransporters item3)
		{
			travellingTransporters.Remove(item3);
		}
		if (o is DestroyedSettlement item4)
		{
			destroyedSettlements.Remove(item4);
		}
		if (o is RoutePlannerWaypoint item5)
		{
			routePlannerWaypoints.Remove(item5);
		}
		if (o is MapParent item6)
		{
			mapParents.Remove(item6);
		}
		if (o is Site item7)
		{
			sites.Remove(item7);
		}
		if (o is PeaceTalks item8)
		{
			peaceTalks.Remove(item8);
		}
		ExpandableLandmarksUtility.Notify_WorldObjectsChanged();
	}

	private void Recache()
	{
		worldObjectsHashSet.Clear();
		layerWorldObjects.Clear();
		caravans.Clear();
		settlements.Clear();
		travellingTransporters.Clear();
		settlementBases.Clear();
		destroyedSettlements.Clear();
		routePlannerWaypoints.Clear();
		mapParents.Clear();
		sites.Clear();
		peaceTalks.Clear();
		for (int i = 0; i < worldObjects.Count; i++)
		{
			AddToCache(worldObjects[i]);
		}
	}

	public bool Contains(WorldObject o)
	{
		if (o == null)
		{
			return false;
		}
		return worldObjectsHashSet.Contains(o);
	}

	public IEnumerable<WorldObject> ObjectsAt(PlanetTile tileID)
	{
		if (!tileID.Valid)
		{
			yield break;
		}
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].Tile == tileID)
			{
				yield return worldObjects[i];
			}
		}
	}

	public bool AnyWorldObjectOnLayer(PlanetLayer layer)
	{
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].Tile.Layer == layer)
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyWorldObjectAt(PlanetTile tile)
	{
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].Tile == tile)
			{
				return true;
			}
		}
		return false;
	}

	public bool AnyWorldObjectAt<T>(PlanetTile tile) where T : WorldObject
	{
		return WorldObjectAt<T>(tile) != null;
	}

	public T WorldObjectAt<T>(PlanetTile tile) where T : WorldObject
	{
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].Tile == tile && worldObjects[i] is T)
			{
				return worldObjects[i] as T;
			}
		}
		return null;
	}

	public bool TryGetWorldObjectAt<T>(PlanetTile tile, out T wo) where T : WorldObject
	{
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].Tile == tile && worldObjects[i] is T)
			{
				wo = worldObjects[i] as T;
				return true;
			}
		}
		wo = null;
		return false;
	}

	public bool AnyWorldObjectAt(PlanetTile tile, WorldObjectDef def)
	{
		return WorldObjectAt(tile, def) != null;
	}

	public WorldObject WorldObjectAt(PlanetTile tile, WorldObjectDef def)
	{
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].Tile == tile && worldObjects[i].def == def)
			{
				return worldObjects[i];
			}
		}
		return null;
	}

	public bool AnySettlementAt(PlanetTile tile)
	{
		return SettlementAt(tile) != null;
	}

	public Settlement SettlementAt(PlanetTile tile)
	{
		for (int i = 0; i < settlements.Count; i++)
		{
			if (settlements[i].Tile == tile)
			{
				return settlements[i];
			}
		}
		return null;
	}

	public bool AnySettlementBaseAt(PlanetTile tile)
	{
		return SettlementBaseAt(tile) != null;
	}

	public Settlement SettlementBaseAt(PlanetTile tile)
	{
		for (int i = 0; i < settlementBases.Count; i++)
		{
			if (settlementBases[i].Tile == tile)
			{
				return settlementBases[i];
			}
		}
		return null;
	}

	public bool AnySiteAt(PlanetTile tile)
	{
		return SiteAt(tile) != null;
	}

	public Site SiteAt(PlanetTile tile)
	{
		for (int i = 0; i < sites.Count; i++)
		{
			if (sites[i].Tile == tile)
			{
				return sites[i];
			}
		}
		return null;
	}

	public bool AnyDestroyedSettlementAt(PlanetTile tile)
	{
		return DestroyedSettlementAt(tile) != null;
	}

	public DestroyedSettlement DestroyedSettlementAt(PlanetTile tile)
	{
		for (int i = 0; i < destroyedSettlements.Count; i++)
		{
			if (destroyedSettlements[i].Tile == tile)
			{
				return destroyedSettlements[i];
			}
		}
		return null;
	}

	public bool AnyMapParentAt(PlanetTile tile)
	{
		return MapParentAt(tile) != null;
	}

	public MapParent MapParentAt(PlanetTile tile)
	{
		for (int i = 0; i < mapParents.Count; i++)
		{
			if (mapParents[i].Tile == tile)
			{
				return mapParents[i];
			}
		}
		return null;
	}

	public bool AnyWorldObjectOfDefAt(WorldObjectDef def, PlanetTile tile)
	{
		return WorldObjectOfDefAt(def, tile) != null;
	}

	public WorldObject WorldObjectOfDefAt(WorldObjectDef def, PlanetTile tile)
	{
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].def == def && worldObjects[i].Tile == tile)
			{
				return worldObjects[i];
			}
		}
		return null;
	}

	public bool AnyGeneratedWorldLocationAt(PlanetTile tile)
	{
		for (int i = 0; i < worldObjects.Count; i++)
		{
			if (worldObjects[i].isGeneratedLocation && worldObjects[i].Tile == tile)
			{
				return true;
			}
		}
		return false;
	}

	public Caravan PlayerControlledCaravanAt(PlanetTile tile)
	{
		for (int i = 0; i < caravans.Count; i++)
		{
			if (caravans[i].Tile == tile && caravans[i].IsPlayerControlled)
			{
				return caravans[i];
			}
		}
		return null;
	}

	public bool AnySettlementBaseAtOrAdjacent(PlanetTile tile)
	{
		WorldGrid worldGrid = Find.WorldGrid;
		for (int i = 0; i < settlementBases.Count; i++)
		{
			Settlement settlement = settlementBases[i];
			if (settlement.Tile.Layer == tile.Layer && worldGrid.IsNeighborOrSame(settlement.Tile, tile))
			{
				return true;
			}
		}
		return false;
	}

	public bool AnySettlementBaseAtOrAdjacent(PlanetTile tile, out WorldObject wo)
	{
		WorldGrid worldGrid = Find.WorldGrid;
		for (int i = 0; i < settlementBases.Count; i++)
		{
			Settlement settlement = settlementBases[i];
			if (settlement.Tile.Layer == tile.Layer && worldGrid.IsNeighborOrSame(settlement.Tile, tile))
			{
				wo = settlement;
				return true;
			}
		}
		wo = null;
		return false;
	}

	public RoutePlannerWaypoint RoutePlannerWaypointAt(PlanetTile tile)
	{
		for (int i = 0; i < routePlannerWaypoints.Count; i++)
		{
			if (routePlannerWaypoints[i].Tile == tile)
			{
				return routePlannerWaypoints[i];
			}
		}
		return null;
	}

	public void GetPlayerControlledCaravansAt(PlanetTile tile, List<Caravan> outCaravans)
	{
		outCaravans.Clear();
		for (int i = 0; i < caravans.Count; i++)
		{
			Caravan caravan = caravans[i];
			if (caravan.Tile == tile && caravan.IsPlayerControlled)
			{
				outCaravans.Add(caravan);
			}
		}
	}
}
