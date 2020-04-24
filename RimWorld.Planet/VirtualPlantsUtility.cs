using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class VirtualPlantsUtility
	{
		private static readonly FloatRange VirtualPlantNutritionRandomFactor = new FloatRange(0.7f, 1f);

		public static bool CanEverEatVirtualPlants(Pawn p)
		{
			return p.RaceProps.Eats(FoodTypeFlags.Plant);
		}

		public static bool CanEatVirtualPlantsNow(Pawn p)
		{
			return CanEatVirtualPlants(p, GenTicks.TicksAbs);
		}

		public static bool CanEatVirtualPlants(Pawn p, int ticksAbs)
		{
			if (p.Tile >= 0 && !p.Dead && p.IsWorldPawn() && CanEverEatVirtualPlants(p))
			{
				return EnvironmentAllowsEatingVirtualPlantsAt(p.Tile, ticksAbs);
			}
			return false;
		}

		public static bool EnvironmentAllowsEatingVirtualPlantsNowAt(int tile)
		{
			return EnvironmentAllowsEatingVirtualPlantsAt(tile, GenTicks.TicksAbs);
		}

		public static bool EnvironmentAllowsEatingVirtualPlantsAt(int tile, int ticksAbs)
		{
			if (!Find.WorldGrid[tile].biome.hasVirtualPlants)
			{
				return false;
			}
			if (GenTemperature.GetTemperatureFromSeasonAtTile(ticksAbs, tile) < 0f)
			{
				return false;
			}
			return true;
		}

		public static void EatVirtualPlants(Pawn p)
		{
			float num = ThingDefOf.Plant_Grass.GetStatValueAbstract(StatDefOf.Nutrition) * VirtualPlantNutritionRandomFactor.RandomInRange;
			p.needs.food.CurLevel += num;
		}

		public static string GetVirtualPlantsStatusExplanationAt(int tile, int ticksAbs)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (ticksAbs == GenTicks.TicksAbs)
			{
				stringBuilder.Append("AnimalsCanGrazeNow".Translate());
			}
			else if (ticksAbs > GenTicks.TicksAbs)
			{
				stringBuilder.Append("AnimalsWillBeAbleToGraze".Translate());
			}
			else
			{
				stringBuilder.Append("AnimalsCanGraze".Translate());
			}
			stringBuilder.Append(": ");
			bool flag = EnvironmentAllowsEatingVirtualPlantsAt(tile, ticksAbs);
			stringBuilder.Append(flag ? "Yes".Translate() : "No".Translate());
			if (flag)
			{
				float? approxDaysUntilPossibleToGraze = GetApproxDaysUntilPossibleToGraze(tile, ticksAbs, untilNoLongerPossibleToGraze: true);
				if (approxDaysUntilPossibleToGraze.HasValue)
				{
					stringBuilder.Append("\n" + "PossibleToGrazeFor".Translate(approxDaysUntilPossibleToGraze.Value.ToString("0.#")));
				}
				else
				{
					stringBuilder.Append("\n" + "PossibleToGrazeForever".Translate());
				}
			}
			else
			{
				if (!Find.WorldGrid[tile].biome.hasVirtualPlants)
				{
					stringBuilder.Append("\n" + "CantGrazeBecauseOfBiome".Translate(Find.WorldGrid[tile].biome.label));
				}
				float? approxDaysUntilPossibleToGraze2 = GetApproxDaysUntilPossibleToGraze(tile, ticksAbs);
				if (approxDaysUntilPossibleToGraze2.HasValue)
				{
					stringBuilder.Append("\n" + "CantGrazeBecauseOfTemp".Translate(approxDaysUntilPossibleToGraze2.Value.ToString("0.#")));
				}
			}
			return stringBuilder.ToString();
		}

		public static float? GetApproxDaysUntilPossibleToGraze(int tile, int ticksAbs, bool untilNoLongerPossibleToGraze = false)
		{
			if (!untilNoLongerPossibleToGraze && !Find.WorldGrid[tile].biome.hasVirtualPlants)
			{
				return null;
			}
			float num = 0f;
			for (int i = 0; i < Mathf.CeilToInt(133.333344f); i++)
			{
				bool flag = EnvironmentAllowsEatingVirtualPlantsAt(tile, ticksAbs + (int)(num * 60000f));
				if ((!untilNoLongerPossibleToGraze && flag) || (untilNoLongerPossibleToGraze && !flag))
				{
					return num;
				}
				num += 0.45f;
			}
			return null;
		}
	}
}
