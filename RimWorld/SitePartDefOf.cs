namespace RimWorld;

[DefOf]
public static class SitePartDefOf
{
	public static SitePartDef PreciousLump;

	public static SitePartDef PossibleUnknownThreatMarker;

	public static SitePartDef BanditCamp;

	[MayRequireIdeology]
	public static SitePartDef WorshippedTerminal;

	[MayRequireIdeology]
	public static SitePartDef AncientComplex;

	[MayRequireIdeology]
	public static SitePartDef AncientAltar;

	[MayRequireIdeology]
	public static SitePartDef Archonexus;

	[MayRequireBiotech]
	public static SitePartDef AncientComplex_Mechanitor;

	[MayRequireOdyssey]
	public static SitePartDef AbandonedSettlement;

	[MayRequireOdyssey]
	public static SitePartDef MechanoidRelay;

	[MayRequireOdyssey]
	public static SitePartDef InsectLair;

	[MayRequireOdyssey]
	public static SitePartDef AncientReactor;

	[MayRequireOdyssey]
	public static SitePartDef OrbitalMechanoidPlatform;

	[MayRequireOdyssey]
	public static SitePartDef OrbitalAncientPlatform;

	[MayRequireOdyssey]
	public static SitePartDef AncientStockpile;

	[MayRequireOdyssey]
	public static SitePartDef CrashedMechanoidPlatform;

	[MayRequireOdyssey]
	public static SitePartDef FrozenTerraformer;

	[MayRequireOdyssey]
	public static SitePartDef OrbitalMechhive;

	[MayRequireOdyssey]
	public static SitePartDef GravshipWreckage;

	[MayRequireOdyssey]
	public static SitePartDef BanditGang;

	static SitePartDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(SitePartDefOf));
	}
}
