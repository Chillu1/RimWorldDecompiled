using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class Alert_ReimplantationAvailable : Alert
{
	private Pawn waitingPawn;

	private Pawn WaitingPawn
	{
		get
		{
			foreach (Map map in Find.Maps)
			{
				foreach (Lord lord in map.lordManager.lords)
				{
					if (lord.CurLordToil is LordToil_ReimplantXenogerm lordToil_ReimplantXenogerm)
					{
						waitingPawn = lordToil_ReimplantXenogerm.Data.target;
						return waitingPawn;
					}
				}
			}
			return null;
		}
	}

	public Alert_ReimplantationAvailable()
	{
		defaultPriority = AlertPriority.High;
		defaultLabel = "AlertReimplantationAvailable".Translate();
		requireBiotech = true;
	}

	public override TaggedString GetExplanation()
	{
		return "AlertReimplantationAvailableDesc".Translate(waitingPawn);
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritIs(WaitingPawn);
	}
}
