namespace RimWorld;

[DefOf]
public static class PawnsArrivalModeDefOf
{
	public static PawnsArrivalModeDef EdgeWalkIn;

	public static PawnsArrivalModeDef EdgeWalkInGroups;

	public static PawnsArrivalModeDef EdgeWalkInDistributed;

	public static PawnsArrivalModeDef CenterDrop;

	public static PawnsArrivalModeDef EdgeDrop;

	public static PawnsArrivalModeDef SpecificDropDebug;

	public static PawnsArrivalModeDef EmergeFromWater;

	public static PawnsArrivalModeDef RandomDrop;

	[MayRequireAnomaly]
	public static PawnsArrivalModeDef EdgeWalkInHateChanters;

	[MayRequireAnomaly]
	public static PawnsArrivalModeDef EdgeWalkInDistributedGroups;

	[MayRequireAnomaly]
	public static PawnsArrivalModeDef EdgeWalkInDarkness;

	static PawnsArrivalModeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PawnsArrivalModeDefOf));
	}
}
