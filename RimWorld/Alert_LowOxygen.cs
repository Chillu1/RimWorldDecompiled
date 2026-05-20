using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_LowOxygen : Alert_Critical
{
	private readonly List<Pawn> pawns = new List<Pawn>();

	private List<Pawn> Pawns
	{
		get
		{
			if (!ModsConfig.OdysseyActive)
			{
				return pawns;
			}
			pawns.Clear();
			foreach (Pawn item in PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer))
			{
				if (!(item.Position.GetVacuum(item.Map) < 0.5f))
				{
					Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.VacuumExposure);
					if (firstHediffOfDef != null && firstHediffOfDef.CurStageIndex >= 2)
					{
						pawns.Add(item);
					}
				}
			}
			return pawns;
		}
	}

	public Alert_LowOxygen()
	{
		defaultLabel = "AlertVacuumExposure".Translate();
	}

	public override TaggedString GetExplanation()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Pawn pawn in pawns)
		{
			stringBuilder.AppendLine("  - " + pawn.NameShortColored.Resolve());
		}
		return "AlertVacuumExposureDesc".Translate(stringBuilder.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Pawns);
	}
}
