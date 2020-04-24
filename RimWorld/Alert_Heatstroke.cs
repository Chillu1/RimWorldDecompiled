using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Heatstroke : Alert
	{
		private List<Pawn> heatstrokePawnsResult = new List<Pawn>();

		private List<Pawn> HeatstrokePawns
		{
			get
			{
				heatstrokePawnsResult.Clear();
				List<Pawn> list = PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					Pawn pawn = list[i];
					if (pawn.health != null && !pawn.RaceProps.Animal && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke, mustBeVisible: true) != null)
					{
						heatstrokePawnsResult.Add(pawn);
					}
				}
				return heatstrokePawnsResult;
			}
		}

		public Alert_Heatstroke()
		{
			defaultLabel = "AlertHeatstroke".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn heatstrokePawn in HeatstrokePawns)
			{
				stringBuilder.AppendLine("  - " + heatstrokePawn.NameShortColored.Resolve());
			}
			return string.Format("AlertHeatstrokeDesc".Translate(), stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(HeatstrokePawns);
		}
	}
}
