using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_ColonistsIdle : Alert
	{
		public const int MinDaysPassed = 1;

		private List<Pawn> idleColonistsResult = new List<Pawn>();

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
						if (!item.mindState.IsIdle)
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
			return "ColonistsIdle".Translate(IdleColonists.Count.ToStringCached());
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn idleColonist in IdleColonists)
			{
				stringBuilder.AppendLine("  - " + idleColonist.NameShortColored.Resolve());
			}
			return "ColonistsIdleDesc".Translate(stringBuilder.ToString());
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
}
