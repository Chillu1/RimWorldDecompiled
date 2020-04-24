using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class WorldObjectsHolder : IExposable
	{
		private List<WorldObject> worldObjects = new List<WorldObject>();

		private HashSet<WorldObject> worldObjectsHashSet = new HashSet<WorldObject>();

		private List<Caravan> caravans = new List<Caravan>();

		private List<Settlement> settlements = new List<Settlement>();

		private List<TravelingTransportPods> travelingTransportPods = new List<TravelingTransportPods>();

		private List<Settlement> settlementBases = new List<Settlement>();

		private List<DestroyedSettlement> destroyedSettlements = new List<DestroyedSettlement>();

		private List<RoutePlannerWaypoint> routePlannerWaypoints = new List<RoutePlannerWaypoint>();

		private List<MapParent> mapParents = new List<MapParent>();

		private List<Site> sites = new List<Site>();

		private List<PeaceTalks> peaceTalks = new List<PeaceTalks>();

		private static List<WorldObject> tmpUnsavedWorldObjects = new List<WorldObject>();

		private static List<WorldObject> tmpWorldObjects = new List<WorldObject>();

		public List<WorldObject> AllWorldObjects => worldObjects;

		public List<Caravan> Caravans => caravans;

		public List<Settlement> Settlements => settlements;

		public List<TravelingTransportPods> TravelingTransportPods => travelingTransportPods;

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
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
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
						Log.Error("Exception spawning WorldObject: " + arg);
						worldObjects.RemoveAt(num2);
					}
				}
			}
		}

		public void Add(WorldObject o)
		{
			if (worldObjects.Contains(o))
			{
				Log.Error("Tried to add world object " + o + " to world, but it's already here.");
				return;
			}
			if (o.Tile < 0)
			{
				Log.Error("Tried to add world object " + o + " but its tile is not set. Setting to 0.");
				o.Tile = 0;
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
				Log.Error("Tried to remove world object " + o + " from world, but it's not here.");
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
				tmpWorldObjects[i].Tick();
			}
		}

		private void AddToCache(WorldObject o)
		{
			worldObjectsHashSet.Add(o);
			if (o is Caravan)
			{
				caravans.Add((Caravan)o);
			}
			if (o is Settlement)
			{
				settlements.Add((Settlement)o);
			}
			if (o is TravelingTransportPods)
			{
				travelingTransportPods.Add((TravelingTransportPods)o);
			}
			if (o is Settlement)
			{
				settlementBases.Add((Settlement)o);
			}
			if (o is DestroyedSettlement)
			{
				destroyedSettlements.Add((DestroyedSettlement)o);
			}
			if (o is RoutePlannerWaypoint)
			{
				routePlannerWaypoints.Add((RoutePlannerWaypoint)o);
			}
			if (o is MapParent)
			{
				mapParents.Add((MapParent)o);
			}
			if (o is Site)
			{
				sites.Add((Site)o);
			}
			if (o is PeaceTalks)
			{
				peaceTalks.Add((PeaceTalks)o);
			}
		}

		private void RemoveFromCache(WorldObject o)
		{
			worldObjectsHashSet.Remove(o);
			if (o is Caravan)
			{
				caravans.Remove((Caravan)o);
			}
			if (o is Settlement)
			{
				settlements.Remove((Settlement)o);
			}
			if (o is TravelingTransportPods)
			{
				travelingTransportPods.Remove((TravelingTransportPods)o);
			}
			if (o is Settlement)
			{
				settlementBases.Remove((Settlement)o);
			}
			if (o is DestroyedSettlement)
			{
				destroyedSettlements.Remove((DestroyedSettlement)o);
			}
			if (o is RoutePlannerWaypoint)
			{
				routePlannerWaypoints.Remove((RoutePlannerWaypoint)o);
			}
			if (o is MapParent)
			{
				mapParents.Remove((MapParent)o);
			}
			if (o is Site)
			{
				sites.Remove((Site)o);
			}
			if (o is PeaceTalks)
			{
				peaceTalks.Remove((PeaceTalks)o);
			}
		}

		private void Recache()
		{
			worldObjectsHashSet.Clear();
			caravans.Clear();
			settlements.Clear();
			travelingTransportPods.Clear();
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

		public IEnumerable<WorldObject> ObjectsAt(int tileID)
		{
			if (tileID < 0)
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

		public bool AnyWorldObjectAt(int tile)
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

		public bool AnyWorldObjectAt<T>(int tile) where T : WorldObject
		{
			return WorldObjectAt<T>(tile) != null;
		}

		public T WorldObjectAt<T>(int tile) where T : WorldObject
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

		public bool AnyWorldObjectAt(int tile, WorldObjectDef def)
		{
			return WorldObjectAt(tile, def) != null;
		}

		public WorldObject WorldObjectAt(int tile, WorldObjectDef def)
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

		public bool AnySettlementAt(int tile)
		{
			return SettlementAt(tile) != null;
		}

		public Settlement SettlementAt(int tile)
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

		public bool AnySettlementBaseAt(int tile)
		{
			return SettlementBaseAt(tile) != null;
		}

		public Settlement SettlementBaseAt(int tile)
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

		public bool AnySiteAt(int tile)
		{
			return SiteAt(tile) != null;
		}

		public Site SiteAt(int tile)
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

		public bool AnyDestroyedSettlementAt(int tile)
		{
			return DestroyedSettlementAt(tile) != null;
		}

		public DestroyedSettlement DestroyedSettlementAt(int tile)
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

		public bool AnyMapParentAt(int tile)
		{
			return MapParentAt(tile) != null;
		}

		public MapParent MapParentAt(int tile)
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

		public bool AnyWorldObjectOfDefAt(WorldObjectDef def, int tile)
		{
			return WorldObjectOfDefAt(def, tile) != null;
		}

		public WorldObject WorldObjectOfDefAt(WorldObjectDef def, int tile)
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

		public Caravan PlayerControlledCaravanAt(int tile)
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

		public bool AnySettlementBaseAtOrAdjacent(int tile)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			for (int i = 0; i < settlementBases.Count; i++)
			{
				if (worldGrid.IsNeighborOrSame(settlementBases[i].Tile, tile))
				{
					return true;
				}
			}
			return false;
		}

		public RoutePlannerWaypoint RoutePlannerWaypointAt(int tile)
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

		public void GetPlayerControlledCaravansAt(int tile, List<Caravan> outCaravans)
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
}
