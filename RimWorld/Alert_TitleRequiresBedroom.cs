using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_TitleRequiresBedroom : Alert
	{
		private List<Pawn> targetsResult = new List<Pawn>();

		public List<Pawn> Targets
		{
			get
			{
				targetsResult.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					foreach (Pawn freeColonist in maps[i].mapPawns.FreeColonists)
					{
						if (freeColonist.royalty != null && freeColonist.royalty.CanRequireBedroom() && freeColonist.royalty.HighestTitleWithBedroomRequirements() != null && !freeColonist.royalty.HasPersonalBedroom())
						{
							targetsResult.Add(freeColonist);
						}
					}
				}
				return targetsResult;
			}
		}

		public Alert_TitleRequiresBedroom()
		{
			defaultLabel = "NeedBedroomAssigned".Translate();
			defaultExplanation = "NeedBedroomAssignedDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(Targets);
		}
	}
}
