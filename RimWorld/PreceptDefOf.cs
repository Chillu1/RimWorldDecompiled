namespace RimWorld;

[DefOf]
public static class PreceptDefOf
{
	[MayRequireRoyalty]
	public static PreceptDef AnimaTreeLinking;

	[MayRequireRoyalty]
	public static PreceptDef ThroneSpeech;

	[MayRequireIdeology]
	public static PreceptDef IdeoBuilding;

	[MayRequireIdeology]
	public static PreceptDef IdeoRelic;

	[MayRequireIdeology]
	public static PreceptDef AnimalVenerated;

	[MayRequireIdeology]
	public static PreceptDef NobleDespisedWeapons;

	[MayRequireIdeology]
	public static PreceptDef Temperature_Tough;

	[MayRequireIdeology]
	public static PreceptDef IdeoRole_Leader;

	[MayRequireIdeology]
	public static PreceptDef IdeoRole_Moralist;

	[MayRequireIdeology]
	public static PreceptDef AgeReversal_Demanded;

	[MayRequireIdeology]
	public static PreceptDef Biosculpting_Accelerated;

	[MayRequireIdeology]
	public static PreceptDef NeuralSupercharge_Preferred;

	[MayRequireIdeology]
	public static PreceptDef Skullspike_Desired;

	[MayRequireIdeology]
	public static PreceptDef MeatEating_Disapproved;

	[MayRequireIdeology]
	public static PreceptDef MeatEating_Horrible;

	[MayRequireIdeology]
	public static PreceptDef MeatEating_Abhorrent;

	[MayRequireIdeology]
	public static PreceptDef MeatEating_NonMeat_Disapproved;

	[MayRequireIdeology]
	public static PreceptDef MeatEating_NonMeat_Horrible;

	[MayRequireIdeology]
	public static PreceptDef MeatEating_NonMeat_Abhorrent;

	[MayRequireIdeology]
	public static PreceptDef Cannibalism_RequiredRavenous;

	[MayRequireIdeology]
	public static PreceptDef Cannibalism_Preferred;

	[MayRequireIdeology]
	public static PreceptDef Cannibalism_RequiredStrong;

	[MayRequireIdeology]
	public static PreceptDef Funeral;

	[MayRequireIdeology]
	public static PreceptDef FuneralNoCorpse;

	[MayRequireIdeology]
	public static PreceptDef RoleChange;

	[MayRequireBiotech]
	public static PreceptDef ChildBirth;

	[MayRequireBiotech]
	public static PreceptDef PreferredXenotype;

	[MayRequireAnomaly]
	public static PreceptDef PsychicRituals_Abhorrent;

	[MayRequireAnomaly]
	public static PreceptDef PsychicRituals_Disapproved;

	[MayRequireAnomaly]
	public static PreceptDef Inhumanizing_Required;

	[MayRequireOdyssey]
	public static PreceptDef GravshipLaunch;

	static PreceptDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(PreceptDefOf));
	}
}
