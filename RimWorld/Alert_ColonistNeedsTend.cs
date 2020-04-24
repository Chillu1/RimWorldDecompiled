using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Alert_ColonistNeedsTend : Alert
	{
		private List<Pawn> needingColonistsResult = new List<Pawn>();

		private List<Pawn> NeedingColonists
		{
			get
			{
				needingColonistsResult.Clear();
				foreach (Pawn item in PawnsFinder.AllMaps_FreeColonistsSpawned)
				{
					if (item.health.HasHediffsNeedingTendByPlayer(forAlert: true))
					{
						Building_Bed building_Bed = item.CurrentBed();
						if ((building_Bed == null || !building_Bed.Medical) && !Alert_ColonistNeedsRescuing.NeedsRescue(item))
						{
							needingColonistsResult.Add(item);
						}
					}
				}
				return needingColonistsResult;
			}
		}

		public Alert_ColonistNeedsTend()
		{
			defaultLabel = "ColonistNeedsTreatment".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override TaggedString GetExplanation()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Pawn needingColonist in NeedingColonists)
			{
				stringBuilder.AppendLine("  - " + needingColonist.NameShortColored.Resolve());
			}
			return "ColonistNeedsTreatmentDesc".Translate(stringBuilder.ToString());
		}

		public override AlertReport GetReport()
		{
			return AlertReport.CulpritsAre(NeedingColonists);
		}
	}
}
