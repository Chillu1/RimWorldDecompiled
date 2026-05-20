namespace RimWorld;

[DefOf]
public static class WorldObjectDefOf
{
	public static WorldObjectDef Caravan;

	public static WorldObjectDef Settlement;

	public static WorldObjectDef AbandonedSettlement;

	public static WorldObjectDef AbandonedCamp;

	public static WorldObjectDef EscapeShip;

	public static WorldObjectDef Ambush;

	public static WorldObjectDef DestroyedSettlement;

	public static WorldObjectDef AttackedNonPlayerCaravan;

	public static WorldObjectDef TravellingTransporters;

	public static WorldObjectDef RoutePlannerWaypoint;

	public static WorldObjectDef Site;

	public static WorldObjectDef PocketMap;

	public static WorldObjectDef Camp;

	[MayRequireRoyalty]
	public static WorldObjectDef TravelingShuttle;

	public static WorldObjectDef Debug_Arena;

	[MayRequireIdeology]
	public static WorldObjectDef Settlement_SecondArchonexusCycle;

	[MayRequireIdeology]
	public static WorldObjectDef Settlement_ThirdArchonexusCycle;

	[MayRequireIdeology]
	public static WorldObjectDef AbandonedArchotechStructures;

	[MayRequireOdyssey]
	public static WorldObjectDef Gravship;

	[MayRequireOdyssey]
	public static WorldObjectDef ClaimableSite;

	[MayRequireOdyssey]
	public static WorldObjectDef ClaimableSpaceSite;

	[MayRequireOdyssey]
	public static WorldObjectDef Mechhive;

	[MayRequireOdyssey]
	public static WorldObjectDef GravshipLaunch;

	[MayRequireOdyssey]
	public static WorldObjectDef AbandonedLandmark;

	static WorldObjectDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(WorldObjectDefOf));
	}
}
