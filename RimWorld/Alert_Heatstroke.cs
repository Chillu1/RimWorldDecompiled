using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_Heatstroke : Alert
{
	private List<Pawn> heatstrokePawnsResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> HeatstrokePawns
	{
		get
		{
			heatstrokePawnsResult.Clear();
			List<Pawn> list = PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				if (pawn.health != null && !pawn.IsAnimal && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke, mustBeVisible: true) != null)
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
		sb.Length = 0;
		foreach (Pawn heatstrokePawn in HeatstrokePawns)
		{
			sb.AppendLine("  - " + heatstrokePawn.NameShortColored.Resolve());
		}
		return string.Format("AlertHeatstrokeDesc".Translate(), sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(HeatstrokePawns);
	}
}
