using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_RoyalNoThroneAssigned : Alert
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
						if (freeColonist.royalty == null || freeColonist.Suspended || !freeColonist.royalty.CanRequireThroneroom())
						{
							continue;
						}
						bool flag = false;
						List<RoyalTitle> allTitlesForReading = freeColonist.royalty.AllTitlesForReading;
						for (int j = 0; j < allTitlesForReading.Count; j++)
						{
							if (!allTitlesForReading[j].def.throneRoomRequirements.NullOrEmpty())
							{
								flag = true;
								break;
							}
						}
						if (flag && freeColonist.ownership.AssignedThrone == null)
						{
							targetsResult.Add(freeColonist);
						}
					}
				}
				return targetsResult;
			}
		}

		public Alert_RoyalNoThroneAssigned()
		{
			defaultLabel = "NeedThroneAssigned".Translate();
			defaultExplanation = "NeedThroneAssignedDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(Targets);
		}
	}
}
