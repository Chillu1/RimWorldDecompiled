using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_StarvationColonists : Alert
{
	private List<Pawn> starvingColonistsResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> StarvingColonists
	{
		get
		{
			starvingColonistsResult.Clear();
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_NoSuspended)
			{
				if (item.needs.food != null && item.needs.food.Starving)
				{
					starvingColonistsResult.Add(item);
				}
			}
			return starvingColonistsResult;
		}
	}

	public Alert_StarvationColonists()
	{
		defaultLabel = "Starvation".Translate();
		defaultPriority = AlertPriority.High;
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (Pawn item in starvingColonistsResult)
		{
			sb.AppendLine("  - " + item.NameShortColored.Resolve());
		}
		return "StarvationDesc".Translate(sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(StarvingColonists);
	}
}
