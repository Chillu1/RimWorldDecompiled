using System.Linq;
using Verse;

namespace RimWorld;

public static class DefGenerator
{
	public const int StandardItemPathCost = 14;

	public static void GenerateImpliedDefs_PreResolve(bool hotReload = false)
	{
		foreach (TerrainDef item in TerrainDefGenerator_Carpet.ImpliedTerrainDefs(hotReload))
		{
			AddImpliedDef(item, hotReload);
		}
		foreach (ThingDef item2 in ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs(hotReload).Concat(ThingDefGenerator_Meat.ImpliedMeatDefs(hotReload)).Concat(ThingDefGenerator_Techprints.ImpliedTechprintDefs(hotReload))
			.Concat(ThingDefGenerator_Corpses.ImpliedCorpseDefs(hotReload)))
		{
			AddImpliedDef(item2, hotReload);
		}
		DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
		foreach (TerrainDef item3 in TerrainDefGenerator_Stone.ImpliedTerrainDefs(hotReload))
		{
			AddImpliedDef(item3, hotReload);
		}
		foreach (RecipeDef item4 in RecipeDefGenerator.ImpliedRecipeDefs(hotReload))
		{
			AddImpliedDef(item4, hotReload);
		}
		foreach (PawnColumnDef item5 in PawnColumnDefGenerator.ImpliedPawnColumnDefs(hotReload))
		{
			AddImpliedDef(item5, hotReload);
		}
		foreach (ThingDef item6 in ThingDefGenerator_Neurotrainer.ImpliedThingDefs(hotReload))
		{
			AddImpliedDef(item6, hotReload);
		}
		foreach (GeneDef item7 in GeneDefGenerator.ImpliedGeneDefs(hotReload))
		{
			AddImpliedDef(item7, hotReload);
		}
		foreach (ThoughtDef item8 in GeneDefGenerator.ImpliedThoughtDefs(hotReload))
		{
			AddImpliedDef(item8, hotReload);
		}
		AnimationDefGenerator_Flying.InitializeNeededDefs(hotReload);
		foreach (GraphicStateDef item9 in AnimationDefGenerator_Flying.ImpliedGraphicStateDefs())
		{
			AddImpliedDef(item9, hotReload);
		}
		foreach (AnimationDef item10 in AnimationDefGenerator_Flying.ImpliedAnimationDefs())
		{
			AddImpliedDef(item10, hotReload);
		}
	}

	public static void GenerateImpliedDefs_PostResolve(bool hotReload = false)
	{
		foreach (KeyBindingCategoryDef item in KeyBindingDefGenerator.ImpliedKeyBindingCategoryDefs(hotReload))
		{
			AddImpliedDef(item, hotReload);
		}
		foreach (KeyBindingDef item2 in KeyBindingDefGenerator.ImpliedKeyBindingDefs(hotReload))
		{
			AddImpliedDef(item2, hotReload);
		}
	}

	public static void AddImpliedDef<T>(T def, bool hotReload = false) where T : Def, new()
	{
		def.generated = true;
		def.ResolveDefNameHash();
		def.modContentPack?.AddDef(def, "ImpliedDefs");
		def.PostLoad();
		if (!hotReload || DefDatabase<T>.GetNamed(def.defName, errorOnFail: false) == null)
		{
			DefDatabase<T>.Add(def);
		}
	}
}
