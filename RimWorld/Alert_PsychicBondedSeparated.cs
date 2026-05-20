using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Alert_PsychicBondedSeparated : Alert
{
	private List<Pawn> targets = new List<Pawn>();

	public Alert_PsychicBondedSeparated()
	{
		defaultLabel = "AlertPsychicBondedPawnsSeparated".Translate();
		requireBiotech = true;
	}

	public override AlertReport GetReport()
	{
		GetTargets();
		return AlertReport.CulpritsAre(targets);
	}

	public override TaggedString GetExplanation()
	{
		return "AlertPsychicBondedPawnsSeparatedDesc".Translate() + ":\n" + targets.Select((Pawn x) => x.NameShortColored.Resolve()).ToLineList("  - ", capitalizeItems: true);
	}

	private void GetTargets()
	{
		targets.Clear();
		foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravellingTransporters_AliveSpawned)
		{
			if (item.RaceProps.Humanlike && item.Faction == Faction.OfPlayer && item.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicBond) is Hediff_PsychicBond bondHediff && !ThoughtWorker_PsychicBondProximity.NearPsychicBondedPerson(item, bondHediff))
			{
				targets.Add(item);
			}
		}
	}
}
