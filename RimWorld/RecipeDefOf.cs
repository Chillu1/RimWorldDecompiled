using Verse;

namespace RimWorld;

[DefOf]
public static class RecipeDefOf
{
	public static RecipeDef RemoveBodyPart;

	public static RecipeDef InstallPegLeg;

	public static RecipeDef SmeltOrDestroyThing;

	public static RecipeDef Sterilize;

	[MayRequireBiotech]
	public static RecipeDef ImplantXenogerm;

	[MayRequireBiotech]
	public static RecipeDef ImplantEmbryo;

	[MayRequireBiotech]
	public static RecipeDef ExtractHemogenPack;

	static RecipeDefOf()
	{
		DefOfHelper.EnsureInitializedInCtor(typeof(RecipeDefOf));
	}
}
