using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Alert_SlavesUnattended : Alert
{
	private List<Pawn> targetsResult = new List<Pawn>();

	private string labelMultiple;

	public List<Pawn> Targets
	{
		get
		{
			targetsResult.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (SlaveRebellionUtility.IsUnattendedByColonists(maps[i]))
				{
					targetsResult.AddRange(maps[i].mapPawns.SlavesOfColonySpawned);
				}
			}
			return targetsResult;
		}
	}

	public Alert_SlavesUnattended()
	{
		defaultLabel = "SlaveUnattendedLabel".Translate();
		labelMultiple = "SlaveUnattendedMultipleLabel".Translate();
		defaultPriority = AlertPriority.High;
		requireIdeology = true;
	}

	public override string GetLabel()
	{
		if (targetsResult.Count != 1)
		{
			return labelMultiple;
		}
		return defaultLabel;
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(Targets);
	}

	public override TaggedString GetExplanation()
	{
		return "SlavesUnattendedDesc".Translate(targetsResult[0]);
	}
}
