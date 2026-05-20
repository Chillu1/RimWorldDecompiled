using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class Alert_CaravanIdle : Alert
{
	private List<Caravan> idleCaravansResult = new List<Caravan>();

	private StringBuilder sb = new StringBuilder();

	private List<Caravan> IdleCaravans
	{
		get
		{
			idleCaravansResult.Clear();
			foreach (Caravan caravan in Find.WorldObjects.Caravans)
			{
				if (caravan.Spawned && caravan.IsPlayerControlled && !caravan.pather.MovingNow && !caravan.CantMove)
				{
					idleCaravansResult.Add(caravan);
				}
			}
			return idleCaravansResult;
		}
	}

	public Alert_CaravanIdle()
	{
		defaultLabel = "CaravanIdle".Translate();
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (Caravan item in idleCaravansResult)
		{
			sb.AppendLine("  - " + item.Label);
		}
		return "CaravanIdleDesc".Translate(sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(IdleCaravans);
	}
}
