namespace RimWorld;

[DefOf]
public static class WorkGiverDefOf
{
	public static WorkGiverDef Repair;

	public static WorkGiverDef ConstructRemoveFloors;

	public static WorkGiverDef FightFires;

	static WorkGiverDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(WorkGiverDefOf));
	}
}
