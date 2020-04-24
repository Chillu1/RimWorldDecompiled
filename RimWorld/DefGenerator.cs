using System.Linq;
using Verse;

namespace RimWorld
{
	public static class DefGenerator
	{
		public static void GenerateImpliedDefs_PreResolve()
		{
			foreach (ThingDef item in ThingDefGenerator_Buildings.ImpliedBlueprintAndFrameDefs().Concat(ThingDefGenerator_Meat.ImpliedMeatDefs()).Concat(ThingDefGenerator_Techprints.ImpliedTechprintDefs())
				.Concat(ThingDefGenerator_Corpses.ImpliedCorpseDefs()))
			{
				AddImpliedDef(item);
			}
			DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.Silent);
			foreach (TerrainDef item2 in TerrainDefGenerator_Stone.ImpliedTerrainDefs())
			{
				AddImpliedDef(item2);
			}
			foreach (RecipeDef item3 in RecipeDefGenerator.ImpliedRecipeDefs())
			{
				AddImpliedDef(item3);
			}
			foreach (PawnColumnDef item4 in PawnColumnDefgenerator.ImpliedPawnColumnDefs())
			{
				AddImpliedDef(item4);
			}
			foreach (ThingDef item5 in NeurotrainerDefGenerator.ImpliedThingDefs())
			{
				AddImpliedDef(item5);
			}
		}

		public static void GenerateImpliedDefs_PostResolve()
		{
			foreach (KeyBindingCategoryDef item in KeyBindingDefGenerator.ImpliedKeyBindingCategoryDefs())
			{
				AddImpliedDef(item);
			}
			foreach (KeyBindingDef item2 in KeyBindingDefGenerator.ImpliedKeyBindingDefs())
			{
				AddImpliedDef(item2);
			}
		}

		public static void AddImpliedDef<T>(T def) where T : Def, new()
		{
			def.generated = true;
			def.modContentPack?.AddDef(def, "ImpliedDefs");
			def.PostLoad();
			DefDatabase<T>.Add(def);
		}
	}
}
