using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Alert_ToxicBuildup : Alert
{
	private const float ToxicBuildupSeverityThreshold = 0.6f;

	private List<Pawn> toxicBuildupColonists = new List<Pawn>();

	private List<Pawn> ToxicBuildupColonists
	{
		get
		{
			toxicBuildupColonists.Clear();
			foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_AliveSpawned_FreeColonists_NoSuspended)
			{
				Hediff firstHediffOfDef = item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ToxicBuildup);
				if (firstHediffOfDef != null && firstHediffOfDef.Severity >= 0.6f)
				{
					toxicBuildupColonists.Add(item);
				}
			}
			return toxicBuildupColonists;
		}
	}

	public Alert_ToxicBuildup()
	{
		defaultLabel = "AlertToxicBuildup".Translate();
		defaultPriority = AlertPriority.High;
		requireBiotech = true;
	}

	public override TaggedString GetExplanation()
	{
		string text = toxicBuildupColonists.Select((Pawn p) => p.NameShortColored.Resolve()).ToLineList(" - ");
		return "AlertToxicBuildupDesc".Translate(text);
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(ToxicBuildupColonists);
	}
}
