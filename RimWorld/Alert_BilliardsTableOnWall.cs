using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_BilliardsTableOnWall : Alert
	{
		private List<Thing> badTablesResult = new List<Thing>();

		private List<Thing> BadTables
		{
			get
			{
				badTablesResult.Clear();
				List<Map> maps = Find.Maps;
				Faction ofPlayer = Faction.OfPlayer;
				for (int i = 0; i < maps.Count; i++)
				{
					List<Thing> list = maps[i].listerThings.ThingsOfDef(ThingDefOf.BilliardsTable);
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].Faction == ofPlayer && !JoyGiver_PlayBilliards.ThingHasStandableSpaceOnAllSides(list[j]))
						{
							badTablesResult.Add(list[j]);
						}
					}
				}
				return badTablesResult;
			}
		}

		public Alert_BilliardsTableOnWall()
		{
			defaultLabel = "BilliardsNeedsSpace".Translate();
			defaultExplanation = "BilliardsNeedsSpaceDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(BadTables);
		}
	}
}
