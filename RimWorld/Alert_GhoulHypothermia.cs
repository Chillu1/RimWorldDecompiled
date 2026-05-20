using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_GhoulHypothermia : Alert_Critical
{
	private List<Pawn> hypothermiaDangerGhoulsResult = new List<Pawn>();

	private List<Pawn> HypothermiaDangerGhouls
	{
		get
		{
			hypothermiaDangerGhoulsResult.Clear();
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_ColonySubhumans_NoSuspended)
			{
				if (item.IsGhoul && !item.SafeTemperatureRange().Includes(item.AmbientTemperature))
				{
					Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
					if (firstHediffOfDef != null && firstHediffOfDef.CurStageIndex >= 3)
					{
						hypothermiaDangerGhoulsResult.Add(item);
					}
				}
			}
			return hypothermiaDangerGhoulsResult;
		}
	}

	public Alert_GhoulHypothermia()
	{
		defaultLabel = "AlertGhoulHypothermia".Translate();
	}

	public override TaggedString GetExplanation()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Pawn item in hypothermiaDangerGhoulsResult)
		{
			stringBuilder.AppendLine("  - " + item.NameShortColored.Resolve());
		}
		return "AlertGhoulHypothermiaDesc".Translate(stringBuilder.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(HypothermiaDangerGhouls);
	}
}
