using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_Exhaustion : Alert
	{
		private List<Pawn> exhaustedColonistsResult = new List<Pawn>();

		private List<Pawn> ExhaustedColonists
		{
			get
			{
				exhaustedColonistsResult.Clear();
				List<Pawn> allMaps_FreeColonistsSpawned = PawnsFinder.AllMaps_FreeColonistsSpawned;
				for (int i = 0; i < allMaps_FreeColonistsSpawned.Count; i++)
				{
					if (allMaps_FreeColonistsSpawned[i].needs.rest != null && allMaps_FreeColonistsSpawned[i].needs.rest.CurCategory == RestCategory.Exhausted)
					{
						exhaustedColonistsResult.Add(allMaps_FreeColonistsSpawned[i]);
					}
				}
				return exhaustedColonistsResult;
			}
		}

		public Alert_Exhaustion()
		{
			defaultLabel = "Exhaustion".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn exhaustedColonist in ExhaustedColonists)
			{
				stringBuilder.AppendLine("  - " + exhaustedColonist.NameShortColored.Resolve());
			}
			return "ExhaustionDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(ExhaustedColonists);
		}
	}
}
