using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnTechHediffsGenerator
	{
		private static List<Thing> emptyIngredientsList = new List<Thing>();

		private static List<ThingDef> tmpGeneratedTechHediffsList = new List<ThingDef>();

		public static void GenerateTechHediffsFor(Pawn pawn)
		{
			float partsMoney = pawn.kindDef.techHediffsMoney.RandomInRange;
			int num = pawn.kindDef.techHediffsMaxAmount;
			if (pawn.kindDef.techHediffsRequired != null)
			{
				foreach (ThingDef item in pawn.kindDef.techHediffsRequired)
				{
					partsMoney -= item.BaseMarketValue;
					num--;
					InstallPart(pawn, item);
				}
			}
			if (pawn.kindDef.techHediffsTags == null || pawn.kindDef.techHediffsChance <= 0f)
			{
				return;
			}
			tmpGeneratedTechHediffsList.Clear();
			for (int i = 0; i < num; i++)
			{
				if (Rand.Value > pawn.kindDef.techHediffsChance)
				{
					break;
				}
				IEnumerable<ThingDef> source = DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.isTechHediff && !tmpGeneratedTechHediffsList.Contains(x) && x.BaseMarketValue <= partsMoney && x.techHediffsTags != null && pawn.kindDef.techHediffsTags.Any((string tag) => x.techHediffsTags.Contains(tag)) && (pawn.kindDef.techHediffsDisallowTags == null || !pawn.kindDef.techHediffsDisallowTags.Any((string tag) => x.techHediffsTags.Contains(tag))));
				if (source.Any())
				{
					ThingDef thingDef = source.RandomElementByWeight((ThingDef w) => w.BaseMarketValue);
					partsMoney -= thingDef.BaseMarketValue;
					InstallPart(pawn, thingDef);
					tmpGeneratedTechHediffsList.Add(thingDef);
				}
			}
		}

		private static void InstallPart(Pawn pawn, ThingDef partDef)
		{
			IEnumerable<RecipeDef> source = DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.IsIngredient(partDef) && pawn.def.AllRecipes.Contains(x));
			if (source.Any())
			{
				RecipeDef recipeDef = source.RandomElement();
				if (recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).Any())
				{
					recipeDef.Worker.ApplyOnPawn(pawn, recipeDef.Worker.GetPartsToApplyOn(pawn, recipeDef).RandomElement(), null, emptyIngredientsList, null);
				}
			}
		}
	}
}
