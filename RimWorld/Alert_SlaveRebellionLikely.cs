using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_SlaveRebellionLikely : Alert
{
	private const float ReportAnySlaveMtbThreshold = 15f;

	private StringBuilder sb = new StringBuilder();

	public Alert_SlaveRebellionLikely()
	{
		defaultLabel = "AlertSlaveRebellionLikely".Translate();
		requireIdeology = true;
	}

	public override TaggedString GetExplanation()
	{
		Map currentMap = Find.CurrentMap;
		if (currentMap == null)
		{
			return string.Empty;
		}
		int num = currentMap.mapPawns.SlavesOfColonySpawned.Count((Pawn pawn) => MTBMeetsRebelliousThreshold(SlaveRebellionUtility.InitiateSlaveRebellionMtbDays(pawn)));
		Pawn mostRebelliousPawn = GetMostRebelliousPawn();
		int numTicks = (int)SlaveRebellionUtility.InitiateSlaveRebellionMtbDays(mostRebelliousPawn) * 60000;
		int numTicks2 = (int)SlaveRebellionUtility.RebellionForAnySlaveInMapMtbDays(currentMap) * 60000;
		sb.Length = 0;
		if (num >= 2)
		{
			sb.Append("AlertSlaveRebellionLikelyRebelliousCount".Translate(num.Named("REBELLIOUSCOUNT")) + " ");
		}
		sb.Append("AlertSlaveRebellionLikelyMostRebellious".Translate(numTicks2.ToStringTicksToPeriodVerbose().Named("COMBINEDTIME"), mostRebelliousPawn.Named("REBEL"), numTicks.ToStringTicksToPeriodVerbose().Named("INDIVIDUALTIME")));
		sb.Append("\n\n" + SlaveRebellionUtility.GetSlaveRebellionMtbCalculationExplanation(mostRebelliousPawn));
		return sb.ToString();
	}

	public override AlertReport GetReport()
	{
		if (Find.CurrentMap == null)
		{
			return false;
		}
		float mtb = SlaveRebellionUtility.RebellionForAnySlaveInMapMtbDays(Find.CurrentMap);
		if (!MTBMeetsRebelliousThreshold(mtb))
		{
			return false;
		}
		return AlertReport.CulpritIs(GetMostRebelliousPawn());
	}

	private Pawn GetMostRebelliousPawn()
	{
		IEnumerable<Pawn> source = Find.CurrentMap.mapPawns.SlavesOfColonySpawned.Where((Pawn pawn) => SlaveRebellionUtility.InitiateSlaveRebellionMtbDays(pawn) > 0f);
		if (source.Any())
		{
			return source.MinBy(SlaveRebellionUtility.InitiateSlaveRebellionMtbDays);
		}
		return null;
	}

	private bool MTBMeetsRebelliousThreshold(float mtb)
	{
		if (15f > mtb)
		{
			return mtb > 0f;
		}
		return false;
	}
}
