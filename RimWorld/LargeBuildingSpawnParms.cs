using Verse;

namespace RimWorld;

public struct LargeBuildingSpawnParms
{
	public ThingDef thingDef;

	public float minDistanceToColonyBuilding;

	public float maxDistanceToColonyBuilding;

	public float minDistanceFromUsedRects;

	public float maxDistanceFromPlayerStartPosition;

	public bool canSpawnOnImpassable;

	public int minDistToEdge;

	public SpawnLocationType attemptSpawnLocationType;

	public bool attemptNotUnderBuildings;

	public bool preferFarFromColony;

	public bool ignoreTerrainAffordance;

	public bool ignoreFoundations;

	public bool allowFogged;

	public IntVec2? overrideSize;

	public static readonly LargeBuildingSpawnParms Default = new LargeBuildingSpawnParms
	{
		maxDistanceToColonyBuilding = 50f,
		canSpawnOnImpassable = true
	};

	public IntVec2 Size => overrideSize ?? thingDef.size;

	public LargeBuildingSpawnParms ForThing(ThingDef thingDef)
	{
		LargeBuildingSpawnParms result = this;
		result.thingDef = thingDef;
		return result;
	}
}
