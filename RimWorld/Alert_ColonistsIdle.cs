using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class Alert_ColonistsIdle : Alert
{
	public const int MinDaysPassed = 1;

	private List<Pawn> idleColonistsResult = new List<Pawn>();

	private StringBuilder sb = new StringBuilder();

	private List<Pawn> IdleColonists
	{
		get
		{
			idleColonistsResult.Clear();
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (!maps[i].IsPlayerHome)
				{
					continue;
				}
				foreach (Pawn item in maps[i].mapPawns.FreeColonistsSpawned)
				{
					if (!item.mindState.IsIdle || item.IsQuestLodger())
					{
						continue;
					}
					if (item.royalty != null)
					{
						RoyalTitle mostSeniorTitle = item.royalty.MostSeniorTitle;
						if (mostSeniorTitle == null || !mostSeniorTitle.def.suppressIdleAlert)
						{
							idleColonistsResult.Add(item);
						}
					}
					else
					{
						idleColonistsResult.Add(item);
					}
				}
			}
			return idleColonistsResult;
		}
	}

	public override string GetLabel()
	{
		if (idleColonistsResult.Count == 1)
		{
			return "ColonistIdle".Translate();
		}
		return "ColonistsIdle".Translate(idleColonistsResult.Count.ToStringCached());
	}

	public override TaggedString GetExplanation()
	{
		sb.Length = 0;
		foreach (Pawn item in idleColonistsResult)
		{
			sb.AppendLine("  - " + item.NameShortColored.Resolve());
		}
		return "ColonistsIdleDesc".Translate(sb.ToString().TrimEndNewlines());
	}

	public override AlertReport GetReport()
	{
		if (GenDate.DaysPassed < 1)
		{
			return false;
		}
		return AlertReport.CulpritsAre(IdleColonists);
	}
}
