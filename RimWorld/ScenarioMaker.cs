using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ScenarioMaker
	{
		private static Scenario scen;

		public static Scenario GeneratingScenario => scen;

		public static Scenario GenerateNewRandomScenario(string seed)
		{
			Rand.PushState();
			Rand.Seed = seed.GetHashCode();
			int @int = Rand.Int;
			int[] array = new int[10];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Rand.Int;
			}
			int int2 = Rand.Int;
			scen = new Scenario();
			scen.Category = ScenarioCategory.CustomLocal;
			scen.name = NameGenerator.GenerateName(RulePackDefOf.NamerScenario);
			scen.description = null;
			scen.summary = null;
			Rand.Seed = @int;
			scen.playerFaction = (ScenPart_PlayerFaction)MakeScenPart(ScenPartDefOf.PlayerFaction);
			scen.parts.Add(MakeScenPart(ScenPartDefOf.ConfigPage_ConfigureStartingPawns));
			scen.parts.Add(MakeScenPart(ScenPartDefOf.PlayerPawnsArriveMethod));
			Rand.Seed = array[0];
			AddCategoryScenParts(scen, ScenPartCategory.PlayerPawnFilter, Rand.RangeInclusive(-1, 2));
			Rand.Seed = array[1];
			AddCategoryScenParts(scen, ScenPartCategory.StartingImportant, Rand.RangeInclusive(0, 2));
			Rand.Seed = array[2];
			AddCategoryScenParts(scen, ScenPartCategory.PlayerPawnModifier, Rand.RangeInclusive(-1, 2));
			Rand.Seed = array[3];
			AddCategoryScenParts(scen, ScenPartCategory.Rule, Rand.RangeInclusive(-2, 3));
			Rand.Seed = array[4];
			AddCategoryScenParts(scen, ScenPartCategory.StartingItem, Rand.RangeInclusive(0, 6));
			Rand.Seed = array[5];
			AddCategoryScenParts(scen, ScenPartCategory.WorldThing, Rand.RangeInclusive(-3, 6));
			Rand.Seed = array[6];
			AddCategoryScenParts(scen, ScenPartCategory.GameCondition, Rand.RangeInclusive(-1, 2));
			Rand.Seed = int2;
			foreach (ScenPart allPart in scen.AllParts)
			{
				allPart.Randomize();
			}
			for (int j = 0; j < scen.parts.Count; j++)
			{
				for (int k = 0; k < scen.parts.Count; k++)
				{
					if (j != k && scen.parts[j].TryMerge(scen.parts[k]))
					{
						scen.parts.RemoveAt(k);
						k--;
						if (j > k)
						{
							j--;
						}
					}
				}
			}
			for (int l = 0; l < scen.parts.Count; l++)
			{
				for (int m = 0; m < scen.parts.Count; m++)
				{
					if (l != m && !scen.parts[l].CanCoexistWith(scen.parts[m]))
					{
						scen.parts.RemoveAt(m);
						m--;
						if (l > m)
						{
							l--;
						}
					}
				}
			}
			foreach (string item in scen.ConfigErrors())
			{
				Log.Error(item);
			}
			Rand.PopState();
			Scenario result = scen;
			scen = null;
			return result;
		}

		private static void AddCategoryScenParts(Scenario scen, ScenPartCategory cat, int count)
		{
			scen.parts.AddRange(RandomScenPartsOfCategory(scen, cat, count));
		}

		private static IEnumerable<ScenPart> RandomScenPartsOfCategory(Scenario scen, ScenPartCategory cat, int count)
		{
			if (count <= 0)
			{
				yield break;
			}
			IEnumerable<ScenPartDef> allowedParts = from d in AddableParts(scen)
				where d.category == cat
				select d;
			int numYielded = 0;
			int numTries = 0;
			do
			{
				if (numYielded >= count || !allowedParts.Any())
				{
					yield break;
				}
				ScenPart scenPart = MakeScenPart(allowedParts.RandomElementByWeight((ScenPartDef d) => d.selectionWeight));
				if (CanAddPart(scen, scenPart))
				{
					yield return scenPart;
					numYielded++;
				}
				numTries++;
			}
			while (numTries <= 100);
			Log.Error("Could not add ScenPart of category " + cat + " to scenario " + scen + " after 50 tries.");
		}

		public static IEnumerable<ScenPartDef> AddableParts(Scenario scen)
		{
			return DefDatabase<ScenPartDef>.AllDefs.Where((ScenPartDef d) => scen.AllParts.Count((ScenPart p) => p.def == d) < d.maxUses);
		}

		private static bool CanAddPart(Scenario scen, ScenPart newPart)
		{
			for (int i = 0; i < scen.parts.Count; i++)
			{
				if (!newPart.CanCoexistWith(scen.parts[i]))
				{
					return false;
				}
			}
			return true;
		}

		public static ScenPart MakeScenPart(ScenPartDef def)
		{
			ScenPart obj = (ScenPart)Activator.CreateInstance(def.scenPartClass);
			obj.def = def;
			return obj;
		}
	}
}
