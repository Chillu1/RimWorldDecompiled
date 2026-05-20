using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_MeatHunger : Alert_Critical
{
	private List<Pawn> targets = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> MeatHungerPawns
	{
		get
		{
			targets.Clear();
			List<Pawn> list = PawnsFinder.AllMaps_SpawnedPawnsInFaction(Faction.OfPlayer);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn pawn = list[i];
				if (pawn.IsGhoul && pawn.needs?.food != null && pawn.needs.food.Starving)
				{
					targets.Add(pawn);
				}
			}
			return targets;
		}
	}

	public Alert_MeatHunger()
	{
		defaultLabel = "AlertMeatHunger".Translate();
		defaultPriority = AlertPriority.High;
		requireAnomaly = true;
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (Pawn target in targets)
		{
			sb.AppendLine("  - " + target.NameShortColored.Resolve());
		}
		return string.Format("{0}:\n{1}\n\n{2}", "AlertMeatHungerDesc".Translate(), sb.ToString().TrimEndNewlines(), "AlertMeatHungerDescAppended".Translate());
	}

	public override AlertReport GetReport()
	{
		return AlertReport.CulpritsAre(MeatHungerPawns);
	}
}
