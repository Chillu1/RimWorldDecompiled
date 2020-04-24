using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class BreakRiskAlertUtility
	{
		private static List<Pawn> pawnsAtRiskExtremeResult = new List<Pawn>();

		private static List<Pawn> pawnsAtRiskMajorResult = new List<Pawn>();

		private static List<Pawn> pawnsAtRiskMinorResult = new List<Pawn>();

		public static List<Pawn> PawnsAtRiskExtreme
		{
			get
			{
				pawnsAtRiskExtremeResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!item.Downed && item.mindState.mentalBreaker.BreakExtremeIsImminent)
					{
						pawnsAtRiskExtremeResult.Add(item);
					}
				}
				return pawnsAtRiskExtremeResult;
			}
		}

		public static List<Pawn> PawnsAtRiskMajor
		{
			get
			{
				pawnsAtRiskMajorResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!item.Downed && item.mindState.mentalBreaker.BreakMajorIsImminent)
					{
						pawnsAtRiskMajorResult.Add(item);
					}
				}
				return pawnsAtRiskMajorResult;
			}
		}

		public static List<Pawn> PawnsAtRiskMinor
		{
			get
			{
				pawnsAtRiskMinorResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (!item.Downed && item.mindState.mentalBreaker.BreakMinorIsImminent)
					{
						pawnsAtRiskMinorResult.Add(item);
					}
				}
				return pawnsAtRiskMinorResult;
			}
		}

		public static string AlertLabel
		{
			get
			{
				int num = PawnsAtRiskExtreme.Count();
				string text;
				if (num > 0)
				{
					text = "BreakRiskExtreme".Translate();
				}
				else
				{
					num = PawnsAtRiskMajor.Count();
					if (num > 0)
					{
						text = "BreakRiskMajor".Translate();
					}
					else
					{
						num = PawnsAtRiskMinor.Count();
						text = "BreakRiskMinor".Translate();
					}
				}
				if (num > 1)
				{
					text = text + " x" + num.ToStringCached();
				}
				return text;
			}
		}

		public static string AlertExplanation
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (PawnsAtRiskExtreme.Any())
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					foreach (Pawn item in PawnsAtRiskExtreme)
					{
						stringBuilder2.AppendLine("  - " + item.NameShortColored.Resolve());
					}
					stringBuilder.Append("BreakRiskExtremeDesc".Translate(stringBuilder2).Resolve());
				}
				if (PawnsAtRiskMajor.Any())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					StringBuilder stringBuilder3 = new StringBuilder();
					foreach (Pawn item2 in PawnsAtRiskMajor)
					{
						stringBuilder3.AppendLine("  - " + item2.NameShortColored.Resolve());
					}
					stringBuilder.Append("BreakRiskMajorDesc".Translate(stringBuilder3).Resolve());
				}
				if (PawnsAtRiskMinor.Any())
				{
					if (stringBuilder.Length != 0)
					{
						stringBuilder.AppendLine();
					}
					StringBuilder stringBuilder4 = new StringBuilder();
					foreach (Pawn item3 in PawnsAtRiskMinor)
					{
						stringBuilder4.AppendLine("  - " + item3.NameShortColored.Resolve());
					}
					stringBuilder.Append("BreakRiskMinorDesc".Translate(stringBuilder4).Resolve());
				}
				stringBuilder.AppendLine();
				stringBuilder.Append("BreakRiskDescEnding".Translate());
				return stringBuilder.ToString();
			}
		}

		public static void Clear()
		{
			pawnsAtRiskExtremeResult.Clear();
			pawnsAtRiskMajorResult.Clear();
			pawnsAtRiskMinorResult.Clear();
		}
	}
}
