using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPathGrid
	{
		public float[] movementDifficulty;

		private int allPathCostsRecalculatedDayOfYear = -1;

		private const float ImpassableMovemenetDificulty = 1000f;

		public const float WinterMovementDifficultyOffset = 2f;

		public const float MaxTempForWinterOffset = 5f;

		private static int DayOfYearAt0Long => GenDate.DayOfYear(GenTicks.TicksAbs, 0f);

		public WorldPathGrid()
		{
			ResetPathGrid();
		}

		public void ResetPathGrid()
		{
			movementDifficulty = new float[Find.WorldGrid.TilesCount];
		}

		public void WorldPathGridTick()
		{
			if (allPathCostsRecalculatedDayOfYear != DayOfYearAt0Long)
			{
				RecalculateAllPerceivedPathCosts();
			}
		}

		public bool Passable(int tile)
		{
			if (!Find.WorldGrid.InBounds(tile))
			{
				return false;
			}
			return movementDifficulty[tile] < 1000f;
		}

		public bool PassableFast(int tile)
		{
			return movementDifficulty[tile] < 1000f;
		}

		public float PerceivedMovementDifficultyAt(int tile)
		{
			return movementDifficulty[tile];
		}

		public void RecalculatePerceivedMovementDifficultyAt(int tile, int? ticksAbs = null)
		{
			if (Find.WorldGrid.InBounds(tile))
			{
				bool num = PassableFast(tile);
				movementDifficulty[tile] = CalculatedMovementDifficultyAt(tile, perceivedStatic: true, ticksAbs);
				if (num != PassableFast(tile))
				{
					Find.WorldReachability.ClearCache();
				}
			}
		}

		public void RecalculateAllPerceivedPathCosts()
		{
			RecalculateAllPerceivedPathCosts(null);
			allPathCostsRecalculatedDayOfYear = DayOfYearAt0Long;
		}

		public void RecalculateAllPerceivedPathCosts(int? ticksAbs)
		{
			allPathCostsRecalculatedDayOfYear = -1;
			for (int i = 0; i < movementDifficulty.Length; i++)
			{
				RecalculatePerceivedMovementDifficultyAt(i, ticksAbs);
			}
		}

		public static float CalculatedMovementDifficultyAt(int tile, bool perceivedStatic, int? ticksAbs = null, StringBuilder explanation = null)
		{
			Tile tile2 = Find.WorldGrid[tile];
			if (explanation != null && explanation.Length > 0)
			{
				explanation.AppendLine();
			}
			if (tile2.biome.impassable || tile2.hilliness == Hilliness.Impassable)
			{
				explanation?.Append("Impassable".Translate());
				return 1000f;
			}
			float num = 0f + tile2.biome.movementDifficulty;
			explanation?.Append(tile2.biome.LabelCap + ": " + tile2.biome.movementDifficulty.ToStringWithSign("0.#"));
			float num2 = HillinessMovementDifficultyOffset(tile2.hilliness);
			float num3 = num + num2;
			if (explanation != null && num2 != 0f)
			{
				explanation.AppendLine();
				explanation.Append(tile2.hilliness.GetLabelCap() + ": " + num2.ToStringWithSign("0.#"));
			}
			return num3 + GetCurrentWinterMovementDifficultyOffset(tile, ticksAbs ?? GenTicks.TicksAbs, explanation);
		}

		public static float GetCurrentWinterMovementDifficultyOffset(int tile, int? ticksAbs = null, StringBuilder explanation = null)
		{
			if (!ticksAbs.HasValue)
			{
				ticksAbs = GenTicks.TicksAbs;
			}
			Vector2 vector = Find.WorldGrid.LongLatOf(tile);
			SeasonUtility.GetSeason(GenDate.YearPercent(ticksAbs.Value, vector.x), vector.y, out var _, out var _, out var _, out var winter, out var _, out var permanentWinter);
			float num = winter + permanentWinter;
			num *= Mathf.InverseLerp(5f, 0f, GenTemperature.GetTemperatureFromSeasonAtTile(ticksAbs.Value, tile));
			if (num > 0.01f)
			{
				float num2 = 2f * num;
				if (explanation != null)
				{
					explanation.AppendLine();
					explanation.Append("Winter".Translate());
					if (num < 0.999f)
					{
						explanation.Append(" (" + num.ToStringPercent("F0") + ")");
					}
					explanation.Append(": ");
					explanation.Append(num2.ToStringWithSign("0.#"));
				}
				return num2;
			}
			return 0f;
		}

		public static bool WillWinterEverAffectMovementDifficulty(int tile)
		{
			int ticksAbs = GenTicks.TicksAbs;
			for (int i = 0; i < 3600000; i += 60000)
			{
				if (GenTemperature.GetTemperatureFromSeasonAtTile(ticksAbs + i, tile) < 5f)
				{
					return true;
				}
			}
			return false;
		}

		private static float HillinessMovementDifficultyOffset(Hilliness hilliness)
		{
			return hilliness switch
			{
				Hilliness.Flat => 0f, 
				Hilliness.SmallHills => 0.5f, 
				Hilliness.LargeHills => 1.5f, 
				Hilliness.Mountainous => 3f, 
				Hilliness.Impassable => 1000f, 
				_ => 0f, 
			};
		}
	}
}
