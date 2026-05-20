using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_StarvationAnimals : Alert
{
	private List<Pawn> starvingAnimalsResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> StarvingAnimals
	{
		get
		{
			starvingAnimalsResult.Clear();
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction_NoCryptosleep)
			{
				if (item.HostFaction == null && !item.RaceProps.Humanlike && item.needs.food != null && (item.needs.food.TicksStarving > 30000 || (item.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true) && item.needs.food.TicksStarving > 5000)))
				{
					starvingAnimalsResult.Add(item);
				}
			}
			return starvingAnimalsResult;
		}
	}

	public Alert_StarvationAnimals()
	{
		defaultLabel = "StarvationAnimals".Translate();
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (Pawn item in starvingAnimalsResult.OrderBy((Pawn a) => a.def.label))
		{
			sb.Append("  - " + item.NameShortColored.Resolve());
			if (item.Name.IsValid && !item.Name.Numerical)
			{
				sb.Append(" (" + item.def.label + ")");
			}
			sb.AppendLine();
		}
		return "StarvationAnimalsDesc".Translate(sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(StarvingAnimals);
	}
}
