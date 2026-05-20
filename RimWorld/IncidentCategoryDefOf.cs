namespace RimWorld;

[DefOf]
public static class IncidentCategoryDefOf
{
	public static IncidentCategoryDef Misc;

	public static IncidentCategoryDef ThreatSmall;

	public static IncidentCategoryDef ThreatBig;

	public static IncidentCategoryDef DiseaseHuman;

	public static IncidentCategoryDef GiveQuest;

	public static IncidentCategoryDef DeepDrillInfestation;

	public static IncidentCategoryDef Special;

	static IncidentCategoryDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(IncidentCategoryDefOf));
	}
}
