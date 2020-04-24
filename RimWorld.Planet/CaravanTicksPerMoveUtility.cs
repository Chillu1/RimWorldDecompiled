using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanTicksPerMoveUtility
	{
		public struct CaravanInfo
		{
			public List<Pawn> pawns;

			public float massUsage;

			public float massCapacity;

			public CaravanInfo(Caravan caravan)
			{
				pawns = caravan.PawnsListForReading;
				massUsage = caravan.MassUsage;
				massCapacity = caravan.MassCapacity;
			}

			public CaravanInfo(Dialog_FormCaravan formCaravanDialog)
			{
				pawns = TransferableUtility.GetPawnsFromTransferables(formCaravanDialog.transferables);
				massUsage = formCaravanDialog.MassUsage;
				massCapacity = formCaravanDialog.MassCapacity;
			}
		}

		private const int MaxPawnTicksPerMove = 150;

		private const int DownedPawnMoveTicks = 450;

		public const float CellToTilesConversionRatio = 340f;

		public const int DefaultTicksPerMove = 3300;

		private const float MoveSpeedFactorAtZeroMass = 2f;

		public static int GetTicksPerMove(Caravan caravan, StringBuilder explanation = null)
		{
			if (caravan == null)
			{
				if (explanation != null)
				{
					AppendUsingDefaultTicksPerMoveInfo(explanation);
				}
				return 3300;
			}
			return GetTicksPerMove(new CaravanInfo(caravan), explanation);
		}

		public static int GetTicksPerMove(CaravanInfo caravanInfo, StringBuilder explanation = null)
		{
			return GetTicksPerMove(caravanInfo.pawns, caravanInfo.massUsage, caravanInfo.massCapacity, explanation);
		}

		public static int GetTicksPerMove(List<Pawn> pawns, float massUsage, float massCapacity, StringBuilder explanation = null)
		{
			if (pawns.Any())
			{
				explanation?.Append("CaravanMovementSpeedFull".Translate() + ":");
				float num = 0f;
				for (int i = 0; i < pawns.Count; i++)
				{
					float a = (pawns[i].Downed || pawns[i].CarriedByCaravan()) ? 450 : pawns[i].TicksPerMoveCardinal;
					a = Mathf.Min(a, 150f) * 340f;
					float num2 = 60000f / a;
					if (explanation != null)
					{
						explanation.AppendLine();
						explanation.Append("  - " + pawns[i].LabelShortCap + ": " + num2.ToString("0.#") + " " + "TilesPerDay".Translate());
						if (pawns[i].Downed)
						{
							explanation.Append(" (" + "DownedLower".Translate() + ")");
						}
						else if (pawns[i].CarriedByCaravan())
						{
							explanation.Append(" (" + "Carried".Translate() + ")");
						}
					}
					num += a / (float)pawns.Count;
				}
				float moveSpeedFactorFromMass = GetMoveSpeedFactorFromMass(massUsage, massCapacity);
				if (explanation != null)
				{
					float num3 = 60000f / num;
					explanation.AppendLine();
					explanation.Append("  " + "Average".Translate() + ": " + num3.ToString("0.#") + " " + "TilesPerDay".Translate());
					explanation.AppendLine();
					explanation.Append("  " + "MultiplierForCarriedMass".Translate(moveSpeedFactorFromMass.ToStringPercent()));
				}
				int num4 = Mathf.Max(Mathf.RoundToInt(num / moveSpeedFactorFromMass), 1);
				if (explanation != null)
				{
					float num5 = 60000f / (float)num4;
					explanation.AppendLine();
					explanation.Append("  " + "FinalCaravanPawnsMovementSpeed".Translate() + ": " + num5.ToString("0.#") + " " + "TilesPerDay".Translate());
				}
				return num4;
			}
			if (explanation != null)
			{
				AppendUsingDefaultTicksPerMoveInfo(explanation);
			}
			return 3300;
		}

		private static float GetMoveSpeedFactorFromMass(float massUsage, float massCapacity)
		{
			if (massCapacity <= 0f)
			{
				return 1f;
			}
			float t = massUsage / massCapacity;
			return Mathf.Lerp(2f, 1f, t);
		}

		private static void AppendUsingDefaultTicksPerMoveInfo(StringBuilder sb)
		{
			sb.Append("CaravanMovementSpeedFull".Translate() + ":");
			float num = 18.181818f;
			sb.AppendLine();
			sb.Append("  " + "Default".Translate() + ": " + num.ToString("0.#") + " " + "TilesPerDay".Translate());
		}
	}
}
