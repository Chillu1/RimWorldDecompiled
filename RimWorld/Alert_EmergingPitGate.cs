using System.Linq;
using Verse;

namespace RimWorld;

public class Alert_EmergingPitGate : Alert
{
	private BuildingGroundSpawner pitGate;

	private BuildingGroundSpawner EmergingPitGate
	{
		get
		{
			pitGate = Find.CurrentMap?.listerThings.ThingsOfDef(ThingDefOf.PitGateSpawner).FirstOrDefault() as BuildingGroundSpawner;
			return pitGate;
		}
	}

	public Alert_EmergingPitGate()
	{
		defaultPriority = AlertPriority.High;
		requireAnomaly = true;
	}

	public override string GetLabel()
	{
		return "AlertEmergingPitGate".Translate() + ": " + pitGate.TicksUntilSpawn.ToStringTicksToPeriodVerbose();
	}

	public override TaggedString GetExplanation()
	{
		return string.Format("AlertEmergingPitGateDesc".Translate());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritIs(EmergingPitGate);
	}
}
