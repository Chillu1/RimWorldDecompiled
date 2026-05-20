using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class IdeoBuildingPresenceDemand : IExposable, ILoadReferenceable
{
	public Precept_Building parent;

	public ExpectationDef minExpectation;

	public List<RoomRequirement> roomRequirements;

	public int ID = -1;

	private List<string> roomRequirementsInfoCached;

	private int roomRequirementsInfoCacheTick = -1;

	private Dictionary<Thing, int> buildingRequirementsMetCache = new Dictionary<Thing, int>();

	private int buildingRequirementsMetCacheTick = -1;

	private Alert_IdeoBuildingMissing alertCachedMissing;

	private Alert_IdeoBuildingDisrespected alertCachedDisrespected;

	public List<string> RoomRequirementsInfo
	{
		get
		{
			if (roomRequirements.NullOrEmpty())
			{
				return null;
			}
			if (roomRequirementsInfoCached == null || Find.TickManager.TicksGame - roomRequirementsInfoCacheTick >= 20)
			{
				if (roomRequirementsInfoCached == null)
				{
					roomRequirementsInfoCached = new List<string>();
				}
				else
				{
					roomRequirementsInfoCached.Clear();
				}
				Thing building = ((Find.CurrentMap != null) ? BestBuilding(Find.CurrentMap) : null);
				Room effectiveRoom = GetEffectiveRoom(building);
				foreach (RoomRequirement roomRequirement in roomRequirements)
				{
					if (effectiveRoom == null || !roomRequirement.MetOrDisabled(effectiveRoom))
					{
						roomRequirementsInfoCached.Add(roomRequirement.LabelCap(effectiveRoom));
					}
				}
				roomRequirementsInfoCacheTick = Find.TickManager.TicksGame;
			}
			return roomRequirementsInfoCached;
		}
	}

	public Alert_IdeoBuildingMissing AlertCachedMissingMissing
	{
		get
		{
			if (alertCachedMissing == null)
			{
				alertCachedMissing = new Alert_IdeoBuildingMissing(this);
			}
			return alertCachedMissing;
		}
	}

	public Alert_IdeoBuildingDisrespected AlertCachedMissingDisrespected
	{
		get
		{
			if (alertCachedDisrespected == null)
			{
				alertCachedDisrespected = new Alert_IdeoBuildingDisrespected(this);
			}
			return alertCachedDisrespected;
		}
	}

	public IdeoBuildingPresenceDemand()
	{
	}

	public IdeoBuildingPresenceDemand(Precept_Building precept)
	{
		parent = precept;
		ID = Find.UniqueIDsManager.GetNextPresenceDemandID();
	}

	public bool AppliesTo(Map map)
	{
		if (!map.IsPlayerHome)
		{
			return false;
		}
		if (parent.ThingDef.ritualFocus != null && parent.ThingDef.ritualFocus.consumable)
		{
			return false;
		}
		if (!DebugSettings.activateAllBuildingDemands)
		{
			return ExpectationsUtility.CurrentExpectationFor(map).order >= minExpectation.order;
		}
		return true;
	}

	public bool BuildingPresent(Map map)
	{
		Faction playerFact = Faction.OfPlayer;
		return map.listerThings.ThingsOfDef(parent.ThingDef).Any((Thing t) => t is Building building && building.StyleSourcePrecept == parent && building.Faction == playerFact);
	}

	public bool RequirementsSatisfied(Map map)
	{
		if (roomRequirements.NullOrEmpty())
		{
			return true;
		}
		foreach (Thing item in map.listerThings.ThingsOfDef(parent.ThingDef))
		{
			if (item is Building building && building.StyleSourcePrecept == parent && building.GetRoom() != null && NumRequirementsMet(building) == roomRequirements.Count)
			{
				return true;
			}
		}
		return false;
	}

	private int NumRequirementsMet(Thing building)
	{
		int ticksGame = Find.TickManager.TicksGame;
		if (buildingRequirementsMetCacheTick != ticksGame)
		{
			buildingRequirementsMetCache.Clear();
			buildingRequirementsMetCacheTick = ticksGame;
		}
		if (buildingRequirementsMetCache.TryGetValue(building, out var value))
		{
			return value;
		}
		value = NumRequirementsMetCalc(building);
		buildingRequirementsMetCache[building] = value;
		return value;
	}

	private int NumRequirementsMetCalc(Thing building)
	{
		if (roomRequirements.NullOrEmpty())
		{
			return 0;
		}
		Room effectiveRoom = GetEffectiveRoom(building);
		if (effectiveRoom == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < roomRequirements.Count; i++)
		{
			if (roomRequirements[i].MetOrDisabled(effectiveRoom))
			{
				num++;
			}
		}
		return num;
	}

	public Room GetEffectiveRoom(Thing building)
	{
		Room room = building?.GetRoom();
		if (room == null || room.PsychologicallyOutdoors)
		{
			return null;
		}
		return room;
	}

	public Thing BestBuilding(Map map, bool ignoreFullyMetRequirements = false)
	{
		Thing result = null;
		int num = -1;
		foreach (Thing item in map.listerThings.ThingsOfDef(parent.ThingDef))
		{
			if (!(item is Building building) || building.StyleSourcePrecept != parent)
			{
				continue;
			}
			int num2 = NumRequirementsMet(building);
			if (!(roomRequirements != null && num2 == roomRequirements.Count && ignoreFullyMetRequirements))
			{
				if (GetEffectiveRoom(building) != null)
				{
					num2++;
				}
				if (num2 > num)
				{
					result = building;
					num = num2;
				}
			}
		}
		return result;
	}

	public IEnumerable<Thing> AllBuildings(Map map)
	{
		foreach (Thing item in map.listerThings.ThingsOfDef(parent.ThingDef))
		{
			if (item is Building building && building.StyleSourcePrecept == parent)
			{
				yield return building;
			}
		}
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref ID, "ID", -1);
		Scribe_Defs.Look(ref minExpectation, "minExpectation");
		Scribe_Collections.Look(ref roomRequirements, "roomRequirements", LookMode.Deep);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			if (GameDataSaveLoader.IsSavingOrLoadingExternalIdeo)
			{
				ID = Find.UniqueIDsManager.GetNextPresenceDemandID();
			}
			if (roomRequirements != null && roomRequirements.RemoveAll((RoomRequirement x) => x == null) != 0)
			{
				Log.Error("Removing null room requirements.");
			}
		}
	}

	public string GetUniqueLoadID()
	{
		return "IdeoBuildingPresenceDemand_" + ID;
	}

	public IdeoBuildingPresenceDemand Copy()
	{
		return new IdeoBuildingPresenceDemand
		{
			ID = ID,
			parent = parent,
			minExpectation = minExpectation,
			roomRequirements = roomRequirements
		};
	}
}
