using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_StarvationColonists : Alert
	{
		private List<Pawn> starvingColonistsResult = new List<Pawn>();

		private List<Pawn> StarvingColonists
		{
			get
			{
				starvingColonistsResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep)
				{
					if (item.needs.food != null && item.needs.food.Starving)
					{
						starvingColonistsResult.Add(item);
					}
				}
				return starvingColonistsResult;
			}
		}

		public Alert_StarvationColonists()
		{
			defaultLabel = "Starvation".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn starvingColonist in StarvingColonists)
			{
				stringBuilder.AppendLine("  - " + starvingColonist.NameShortColored.Resolve());
			}
			return "StarvationDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(StarvingColonists);
		}
	}
}
