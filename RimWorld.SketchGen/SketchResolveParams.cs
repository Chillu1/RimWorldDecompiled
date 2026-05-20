using System.Collections.Generic;
using Verse;

namespace RimWorld.SketchGen;

public struct SketchResolveParams
{
	public Sketch sketch;

	public CellRect? rect;

	public bool? allowWood;

	public float? points;

	public float? totalPoints;

	public float? chance;

	public int? symmetryOrigin;

	public bool? symmetryVertical;

	public bool? symmetryOriginIncluded;

	public bool? symmetryClear;

	public bool? connectedGroupsSameStuff;

	public ThingDef assignRandomStuffTo;

	public ThingDef cornerThing;

	public bool? floorFillRoomsOnly;

	public bool? singleFloorType;

	public bool? onlyStoneFloors;

	public ThingDef thingCentral;

	public ThingDef wallEdgeThing;

	public IntVec2? monumentSize;

	public bool? monumentOpen;

	public bool? allowMonumentDoors;

	public ThingFilter allowedMonumentThings;

	public Map useOnlyStonesAvailableOnMap;

	public bool? allowConcrete;

	public bool? allowFlammableWalls;

	public bool? onlyBuildableByPlayer;

	public bool? addFloors;

	public bool? requireFloor;

	public IntVec2? mechClusterSize;

	public bool? mechClusterDormant;

	public Map mechClusterForMap;

	public bool? forceNoConditionCauser;

	public IntVec2? utilityBuildingSize;

	public float? destroyChanceExp;

	public IntVec2? landingPadSize;

	private Dictionary<string, object> custom;

	public void SetCustom<T>(string name, T obj, bool inherit = false)
	{
		ResolveParamsUtility.SetCustom(ref custom, name, obj, inherit);
	}

	public void RemoveCustom(string name)
	{
		ResolveParamsUtility.RemoveCustom(ref custom, name);
	}

	public bool TryGetCustom<T>(string name, out T obj)
	{
		return ResolveParamsUtility.TryGetCustom<T>(custom, name, out obj);
	}

	public T GetCustom<T>(string name)
	{
		return ResolveParamsUtility.GetCustom<T>(custom, name);
	}

	public override string ToString()
	{
		return "sketch=" + ((sketch != null) ? sketch.ToString() : "null") + ", rect=" + (rect.HasValue ? rect.ToString() : "null") + ", allowWood=" + (allowWood.HasValue ? allowWood.ToString() : "null") + ", custom=" + ((custom != null) ? custom.Count.ToString() : "null") + ", symmetryOrigin=" + (symmetryOrigin.HasValue ? symmetryOrigin.ToString() : "null") + ", symmetryVertical=" + (symmetryVertical.HasValue ? symmetryVertical.ToString() : "null") + ", symmetryOriginIncluded=" + (symmetryOriginIncluded.HasValue ? symmetryOriginIncluded.ToString() : "null") + ", symmetryClear=" + (symmetryClear.HasValue ? symmetryClear.ToString() : "null") + ", connectedGroupsSameStuff=" + (connectedGroupsSameStuff.HasValue ? connectedGroupsSameStuff.ToString() : "null") + ", assignRandomStuffTo=" + ((assignRandomStuffTo != null) ? assignRandomStuffTo.ToString() : "null") + ", cornerThing=" + ((cornerThing != null) ? cornerThing.ToString() : "null") + ", floorFillRoomsOnly=" + (floorFillRoomsOnly.HasValue ? floorFillRoomsOnly.ToString() : "null") + ", singleFloorType=" + (singleFloorType.HasValue ? singleFloorType.ToString() : "null") + ", onlyStoneFloors=" + (onlyStoneFloors.HasValue ? onlyStoneFloors.ToString() : "null") + ", thingCentral=" + ((thingCentral != null) ? thingCentral.ToString() : "null") + ", wallEdgeThing=" + ((wallEdgeThing != null) ? wallEdgeThing.ToString() : "null") + ", monumentSize=" + (monumentSize.HasValue ? monumentSize.ToString() : "null") + ", monumentOpen=" + (monumentOpen.HasValue ? monumentOpen.ToString() : "null") + ", allowMonumentDoors=" + (allowMonumentDoors.HasValue ? allowMonumentDoors.ToString() : "null") + ", allowedMonumentThings=" + ((allowedMonumentThings != null) ? allowedMonumentThings.ToString() : "null") + ", useOnlyStonesAvailableOnMap=" + ((useOnlyStonesAvailableOnMap != null) ? useOnlyStonesAvailableOnMap.ToString() : "null") + ", allowConcrete=" + (allowConcrete.HasValue ? allowConcrete.ToString() : "null") + ", allowFlammableWalls=" + (allowFlammableWalls.HasValue ? allowFlammableWalls.ToString() : "null") + ", onlyBuildableByPlayer=" + (onlyBuildableByPlayer.HasValue ? onlyBuildableByPlayer.ToString() : "null") + ", addFloor=" + (addFloors.HasValue ? addFloors.ToString() : "null") + ", requireFloor=" + (requireFloor.HasValue ? requireFloor.ToString() : "null") + ", mechClusterSize=" + (mechClusterSize.HasValue ? mechClusterSize.ToString() : "null") + ", mechClusterDormant=" + (mechClusterDormant.HasValue ? mechClusterDormant.ToString() : "null") + ", mechClusterForMap=" + ((mechClusterForMap != null) ? mechClusterForMap.ToString() : "null");
	}
}
