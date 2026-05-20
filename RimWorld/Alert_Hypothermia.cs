using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_Hypothermia : Alert_Critical
{
	private List<Pawn> hypothermiaDangerColonistsResult = new List<Pawn>();

	private List<Pawn> HypothermiaDangerColonists
	{
		get
		{
			hypothermiaDangerColonistsResult.Clear();
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_NoSuspended)
			{
				if (!item.SafeTemperatureRange().Includes(item.AmbientTemperature))
				{
					Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia);
					if (firstHediffOfDef != null && firstHediffOfDef.CurStageIndex >= 3)
					{
						hypothermiaDangerColonistsResult.Add(item);
					}
				}
			}
			return hypothermiaDangerColonistsResult;
		}
	}

	public Alert_Hypothermia()
	{
		defaultLabel = "AlertHypothermia".Translate();
	}

	public override TaggedString GetExplanation()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Pawn item in hypothermiaDangerColonistsResult)
		{
			stringBuilder.AppendLine("  - " + item.NameShortColored.Resolve());
		}
		return "AlertHypothermiaDesc".Translate(stringBuilder.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(HypothermiaDangerColonists);
	}
}
